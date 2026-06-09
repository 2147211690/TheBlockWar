using System.Linq;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

[GlobalClass]
public partial class GameView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public GameViewNodeContainer DrawContainer { get; private set; } = null!;
    [Export] public GameViewStyle Style { get; set; } = null!;
    [Export] public TilesView TilesView { get; set; } = null!;
    [Export] public UnitsView UnitsView { get; set; } = null!;
    [Export] public EventsView EventsView { get; set; } = null!;
    [Export] public AnimationView AnimationView { get; set; } = null!;
    [Export] public Camera2D SubCamera { get; set; } = null!;
    [Export] public SubViewport SubViewport { get; set; } = null!;
    public IPlayer? Player { get; set; }
    public ViewPools ViewPools { get; private set; } = null!;
    public bool IsRunning { get; private set; }

    private Dictionary<long, ReadOnlySimObject> _runningSimulations = new();
    private List<ReadOnlySimObject> _prepareSimulations = new();
    private EventScheduler? _eventScheduler;
    private Queue<(IReadOnlyList<ReadOnlySimObject> simulations, int? delay)> _simulationCache = new();
    private Vector2I? _previousMouseCoords = null;
    private Vector2I _currentCoords;
    private bool _refocus = false;
    private Queue<int> _nextDelays = new();
    private int? _currentDelay;
    private SimResultHandler _simResultHandler;

    public GameView()
    {
        _simResultHandler = new(HandleSimStep);
    }

    public override void _Ready()
    {
        base._Ready();
        ViewPools = new ViewPools(DrawContainer, Style);
        SubViewport.World2D = GetTree().Root.World2D;
    }

    public void Bind(EventScheduler eventScheduler)
    {
        _eventScheduler = eventScheduler;
        _eventScheduler.SimulationUpdated += EventSchedulerOnSimulationUpdated;
        _eventScheduler.OtherStateUpdated += EventSchedulerOnOtherStateUpdated;
        _eventScheduler.NextDelay += EventSchedulerOnNextDelay;
        _eventScheduler.GameStateReload += EventSchedulerOnGameStateReload;


        TreeExiting += () =>
        {
            _eventScheduler.SimulationUpdated -= EventSchedulerOnSimulationUpdated;
            _eventScheduler.OtherStateUpdated -= EventSchedulerOnOtherStateUpdated;
            _eventScheduler.NextDelay -= EventSchedulerOnNextDelay;
        };
    }
    
    private void EventSchedulerOnGameStateReload(object? sender, IReadOnlyGameState e)
    {
        TilesView.ClearAllTile();
        UnitsView.ClearAllUnit();
        var size = e.Size;
        for (int i = 0; i < size.X; i++)
        {
            for(int j = 0; j < size.Y; j++)
            {
                var tile = e.TileMapState.GetTile(new Vector2I(i, j));
                TilesView.PlaceTile(new(i, j), tile.Name).Forget();
                foreach (var region in tile.Regions)
                {
                    //TODO: 绘制区域
                }
            }
        }

        foreach (var unit in e.UnitMapState.GetAllUnits())
        {
            var view = UnitsView.CreateOrGetUnit(unit.Coords, unit, GetTeamType(unit.Team))!;
            view.PlayPlaceDownAnim().Forget();
        }
    }

    private void EventSchedulerOnNextDelay(object? sender, int e)
    {
        _nextDelays.Enqueue(e);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (IsRunning) return;
        switch (@event)
        {
            case InputEventMouseMotion e:
                _currentCoords = TilesView.GlobalMouseToTileCoords(GetGlobalMousePosition());
                if (_previousMouseCoords is null || _currentCoords != _previousMouseCoords)
                {
                    _previousMouseCoords = _currentCoords;
                    Player?.OnInput(new PlayerInputMouseCoords(_currentCoords));
                }

                break;
            case InputEventMouseButton e:
                Player?.OnInput(new PlayerInputClick(_currentCoords, e.ButtonMask));
                break;
        }
    }

    public TeamTypeEnum GetTeamType(int team)
    {
        return team.GetTeamTypeRelativeToPlayer(_eventScheduler);
    }
    private void EventSchedulerOnOtherStateUpdated(object? sender, EventScheduler.OtherStateUpdateTypeEnum e)
    {
        switch (e)
        {
            case EventScheduler.OtherStateUpdateTypeEnum.FocusedUnitId:
                UnitsView.FocusUnit(_eventScheduler!.FocusedUnitId);
                SubCamera.Position = TilesView.GetRealPosition(
                    UnitsView.GetUnitView(_eventScheduler!.FocusedUnitId)?.WishCoords ?? _currentCoords,
                    Layer.Underground);
                break;
            case EventScheduler.OtherStateUpdateTypeEnum.CoordsRegion:
                TilesView.DrawRegions(_eventScheduler!.CoordsRegions.Values,
                    _eventScheduler.GameState.TileMapState.Size);
                break;
        }
    }

    private void EventSchedulerOnSimulationUpdated(object? sender, List<ReadOnlySimObject> e)
    {
        var hasDelay = _nextDelays.TryDequeue(out var delay);
        SetSimulations(e, hasDelay ? delay : null);
    }

    /// <param name="simulations">确保传入有序的模拟结果</param>
    private void SetSimulations(IReadOnlyList<ReadOnlySimObject> simulations, int? delay = null)
    {
        if (IsRunning)
        {
            Logger.Info("缓存操作结果");
            _simulationCache.Enqueue((simulations, delay));
            return;
        }
        foreach (var simulation in simulations)
        {
            if (simulation.Layer == SimulationLayer.Running)
                _runningSimulations.Add(simulation.SourceEventId, simulation);
            else
                _prepareSimulations.Add(simulation);
        }

        IsRunning = true;
        Run().Forget();

        async GDTaskVoid Run()
        {
            await StartSimulations(delay);
            RunCompleted();
        }
    }

    private void RunCompleted()
    {
        IsRunning = false;
        //回收
        _prepareSimulations.Clear();
        _runningSimulations.Clear();
        
        if (_simulationCache.TryDequeue(out var simulations))
        {
            _currentDelay = simulations.delay;
            SetSimulations(simulations.simulations, simulations.delay);
        }
        else
        {
            if (_refocus) //如果完全完成了且运行过模拟,则重置下关注的坐标
            {
                Player?.OnInput(new PlayerInputMouseCoords(_currentCoords));
                _refocus = false;
            }
        }
    }


    private async GDTask StartSimulations(int? delay = null)
    {
        ClearDraw();
        DrawEvents(_prepareSimulations);
        if (_runningSimulations.Count > 0)
        {
            _refocus = true;
            await RunSimulations(_runningSimulations.Values);
        }
        if (delay != null) await GDTask.Delay(delay.Value);
    }

    private GDTask HandleSimStep(ReadOnlySimObject sim, StepResult step)
    {
        var id = step.GetHashCode();
        Logger.Debug("处理事件开始->id:{0}-type:{1}-time:{2}", id, step.GetType(), Time.GetTicksMsec() / 100);
        return step.ViewRun(this, sim);
        /*switch (step)
        {
            case PlaceTileStep placeStep:
                await TilesView.PlaceTile(placeStep.Apply.Coords, placeStep.Apply.TileName);
                UnitsView.UpdateUnitPosition(placeStep.Apply.Coords);
                break;
            case DropTileStep dropStep:
                await TilesView.DropTile(dropStep.Apply.Coords);
                UnitsView.UpdateUnitPosition(dropStep.Apply.Coords);
                break;
            case PlaceUnitStep placeStep:
                if (placeStep.Valid)
                {
                    var u = UnitFactory.CreateUnit(placeStep.Apply.UnitName, placeStep.Apply.Team, 
                        placeStep.UnitState.Id, placeStep.Apply.Coords);
                    await UnitsView.PlaceUnit(
                        placeStep.Apply.Coords,
                        u,
                        placeStep.Apply.Team.GetTeamTypeRelativeToPlayer(_eventScheduler!));
                }

                break;
            case MoveUnitStep moveStep:
                if (moveStep.Valid) UnitsView.MoveUnit(moveStep.UnitId, moveStep.Apply.To);
                UnitsView.UpdateUnitPosition(moveStep.Apply.To);
                break;
            case DamageStep damageStep:
                UnitsView.GetUnitView(damageStep.UnitId)?.HealthBar.CurrentHealth -= damageStep.ValidDamage;
                break;
            case CureStep cureStep:
                UnitsView.GetUnitView(cureStep.UnitId)?.HealthBar.CurrentHealth += cureStep.ValidCure;
                break;
            case MoveUnitPathStep movePathStep:
                if (movePathStep.Valid)
                    await UnitsView.MovePathUnit(movePathStep.UnitId, movePathStep.Apply.Coords, movePathStep.Apply.Path,
                        movePathStep.TargetIndex);
                break;
            case DropUnitStep dropStep:
                await UnitsView.DropUnit(dropStep.UnitId);
                break;
            case PushUnitStep pushStep:
                await UnitsView.PushUnit(pushStep.UnitId, pushStep.Apply.Dir, pushStep.Valid);
                break;
            case ModifyUnitBuffStep modifyUnitBuffStep:
                if (modifyUnitBuffStep.Operation == BuffOperation.AddBuff)
                {
                    UnitsView.GetUnitView(modifyUnitBuffStep.UnitId)?.BuffView
                        .AddVisibleBuff(modifyUnitBuffStep.Apply.BuffName);
                }
                else if (modifyUnitBuffStep.Operation == BuffOperation.RemoveBuff)
                {
                    UnitsView.GetUnitView(modifyUnitBuffStep.UnitId)?.BuffView
                        .RemoveVisibleBuff(modifyUnitBuffStep.Apply.BuffName);
                }

                break;
            case AnimationStep animationStep:
                await AnimationView.Play(animationStep.Apply.Animation);
                break;
            case DelayStep delayStep:
                await GDTask.Delay(delayStep.Apply.Delay);
                break;
            case DeathStep deathStep:
                UnitsView.GetUnitView(deathStep.UnitId)?.HealthBar.CurrentHealth = 0;
                break;
            case null:
                break;
        }*/
    }

    private GDTask RunSimulations(IEnumerable<ReadOnlySimObject> sims)
    {
        return _simResultHandler.Handle(sims);
    }

    private void ClearDraw()
    {
        EventsView.ClearAll();
    }
    private void DrawEvents(IEnumerable<ReadOnlySimObject> sims)
    {
        foreach (var sim in sims)
        {
            foreach (var step in sim.Steps ?? [])
            {
                step.ViewDraw(this, sim);
            }
        }
        /*EventsView.ClearAll();
        foreach (var simulation in sims)
        {
            var isFocused = simulation.Layer == SimulationLayer.Focused;
            var teamType = TeamTypeEnum.Player; //TODO:每一步都不一样
            var view = EventsView.BeginAddEventView(simulation.SourceEventId, isFocused, teamType);
            if (view is null) continue;
            foreach (var step in simulation.Steps ?? [])
            {
                switch (step)
                {
                    case PlaceTileStep placeStep:
                        //await _tilesView.PlaceTile(placeStep.Coords, placeStep.Tile);
                        break;
                    case DropTileStep dropStep:
                        //await _tilesView.DropTile(dropStep.Coords);
                        break;
                    case PlaceUnitStep placeStep:
                        view.PlaceUnit(placeStep);
                        break;
                    case MoveUnitStep moveStep:
                        view.MoveUnit(moveStep);
                        break;
                    case DamageStep damageStep:
                        view.Damage(damageStep);
                        break;
                    case CureStep cureStep:
                        view.Cure(cureStep);
                        break;
                    case MoveUnitPathStep movePathStep:
                        view.MoveUnitPath(movePathStep);
                        break;
                    case DropUnitStep dropStep:
                        view.DropUnit(dropStep);
                        break;
                    case PushUnitStep pushStep:
                        view.PushUnit(pushStep);
                        break;
                    case ModifyUnitBuffStep modifyUnitBuffStep:
                        view.ModifyUnitBuff(modifyUnitBuffStep);
                        break;
                    case AnimationStep animationStep:
                        view.Animation(animationStep);
                        break;
                    case DeathStep deathStep:
                        view.Death(deathStep);
                        break;
                    case null:
                        break;
                }
            }

            EventsView.EndAddEventView();
        }*/
    }
}