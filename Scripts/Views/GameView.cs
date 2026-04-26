using System.Linq;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;

namespace TheBlockWar.Scripts.Views;

[GlobalClass]
public partial class GameView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public Node2D DrawContainer { get; private set; } = null!;
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

    private Dictionary<long, SimulationContext> _runningSimulations = new();
    private List<SimulationContext> _lastSimulations = new();
    private List<SimulationContext> _prepareSimulations = new();
    private EventScheduler? _eventScheduler;
    private Queue<(IReadOnlyList<SimulationContext> simulations, int? delay)> _simulationCache = new();
    private Vector2I? _previousMouseCoords = null;
    private Vector2I _currentCoords;
    private bool _refocus = false;
    private Queue<int> _nextDelays = new();
    private int? _currentDelay;
    private SimStepHandler _simStepHandler;

    public GameView()
    {
        _simStepHandler = new(HandleSimStep);
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


        TreeExiting += () =>
        {
            _eventScheduler.SimulationUpdated -= EventSchedulerOnSimulationUpdated;
            _eventScheduler.OtherStateUpdated -= EventSchedulerOnOtherStateUpdated;
            _eventScheduler.NextDelay -= EventSchedulerOnNextDelay;
        };
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

    private void EventSchedulerOnSimulationUpdated(object? sender, List<SimulationContext> e)
    {
        var hasDelay = _nextDelays.TryDequeue(out var delay);
        SetSimulations(e, hasDelay ? delay : null);
    }

    /// <param name="simulations">确保传入有序的模拟结果</param>
    private void SetSimulations(IReadOnlyList<SimulationContext> simulations, int? delay = null)
    {
        if (IsRunning)
        {
            Logger.Info("缓存操作结果");
            _simulationCache.Enqueue((simulations, delay));
            return;
        }

        (_lastSimulations, _prepareSimulations) = (_prepareSimulations, _lastSimulations);

        _prepareSimulations.Clear();
        _runningSimulations.Clear();
        foreach (var simulation in simulations)
        {
            if (simulation.Layer == SimulationLayer.Running)
                _runningSimulations.Add(simulation.SourceId, simulation);
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
        //await _simStepHandler.Handle(_runningSimulations.Values);
        //绘制运行前的事件 TODO: 可能优化
        DrawEvents(_prepareSimulations);
        if (_runningSimulations.Count > 0)
        {
            _refocus = true;
            //DrawEvents(_lastSimulations.Where(a => !_runningSimulations.ContainsKey(a.SourceId)));
            await RunSimulations(_runningSimulations.Values);
        }
        if (delay != null) await GDTask.Delay(delay.Value);
    }

    private async GDTask HandleSimStep(SimStep step)
    {
        var id = step.GetHashCode();
        Logger.Debug("处理事件开始->id:{0}-type:{1}-time:{2}", id, step.GetType(), Time.GetTicksMsec() / 100);
        switch (step)
        {
            case PlaceTileStep placeStep:
                await TilesView.PlaceTile(placeStep.Coords, placeStep.Tile);
                UnitsView.UpdateUnitPosition(placeStep.Coords);
                break;
            case DropTileStep dropStep:
                await TilesView.DropTile(dropStep.Coords);
                UnitsView.UpdateUnitPosition(dropStep.Coords);
                break;
            case PlaceUnitStep placeStep:
                if (placeStep.ResultIsValid)
                {
                    var u = UnitFactory.CreateUnit(placeStep.UnitName, placeStep.Team, 
                        placeStep.ResultUnit.Id, placeStep.Coords);
                    await UnitsView.PlaceUnit(
                        placeStep.Coords,
                        u,
                        placeStep.Team.GetTeamTypeRelativeToPlayer(_eventScheduler!));
                }

                break;
            case MoveUnitStep moveStep:
                if (moveStep.ResultIsValid) UnitsView.MoveUnit(moveStep.ResultUnitId, moveStep.To);
                UnitsView.UpdateUnitPosition(moveStep.To);
                break;
            case DamageStep damageStep:
                UnitsView.GetUnitView(damageStep.ResultUnitId)?.HealthBar.CurrentHealth -= damageStep.ResultValidDamage;
                break;
            case CureStep cureStep:
                UnitsView.GetUnitView(cureStep.ResultUnitId)?.HealthBar.CurrentHealth += cureStep.ResultValidCure;
                break;
            case MoveUnitPathStep movePathStep:
                if (movePathStep.ResultIsValid)
                    await UnitsView.MovePathUnit(movePathStep.ResultUnitId, movePathStep.Coords, movePathStep.Path,
                        movePathStep.ResultTargetIndex);
                break;
            case DropUnitStep dropStep:
                await UnitsView.DropUnit(dropStep.ResultUnitId);
                break;
            case PushUnitStep pushStep:
                await UnitsView.PushUnit(pushStep.ResultUnitId, pushStep.PushDir, pushStep.ResultPush);
                break;
            case ModifyUnitBuffStep modifyUnitBuffStep:
                if (modifyUnitBuffStep.Operation == BuffOperation.AddBuff)
                {
                    UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView
                        .AddVisibleBuff(modifyUnitBuffStep.BuffType);
                }
                else if (modifyUnitBuffStep.Operation == BuffOperation.RemoveBuff)
                {
                    UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView
                        .RemoveVisibleBuff(modifyUnitBuffStep.BuffType);
                }

                break;
            case AnimationStep animationStep:
                await AnimationView.Play(animationStep.Animation);
                break;
            case DelayStep delayStep:
                await GDTask.Delay(delayStep.Delay);
                break;
            case null:
                break;
        }
    }

    private GDTask RunSimulations(IEnumerable<SimulationContext> simulations)
    {
        return _simStepHandler.Handle(simulations);
    }

    private void DrawEvents(IEnumerable<SimulationContext> s)
    {
        EventsView.ClearAll();
        foreach (var simulation in s)
        {
            var isFocused = simulation.Layer == SimulationLayer.Focused;
            var teamType = simulation.TeamType;
            var view = EventsView.BeginAddEventView(simulation.SourceId, isFocused, teamType);
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
                    case null:
                        break;
                }
            }

            EventsView.EndAddEventView();
        }
    }
}