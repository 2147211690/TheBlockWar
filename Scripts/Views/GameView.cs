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
    [Export] public GameViewStyle Style { get; set; }
    [Export] public TilesView TilesView { get; set; }
    [Export] public UnitsView UnitsView { get; set; }
    [Export] public EventsView EventsView { get; set; }
    [Export] public AnimationView AnimationView { get; set; }
    [Export] public Camera2D SubCamera { get; set; }
    [Export] public SubViewport SubViewport { get; set; }
    public IPlayer? Player { get; set; }
    public ViewPools ViewPools { get; private set; } = null!;
    public bool IsRunning { get; private set; }
    
    private Dictionary<uint, SimulationResult> _runningSimulations = new();
    private List<SimulationResult> _lastSimulations = new();
    private List<SimulationResult> _prepareSimulations = new();
    private GameScheduler? _gameScheduler;
    private Queue<SimulationResult[]> _simulationCache = new();
    private Vector2I? _previousMouseCoords = null;
    private Vector2I _currentCoords;
    private bool _refocus = false;

    public override void _Ready()
    {
        base._Ready();
        ViewPools = new ViewPools(DrawContainer, Style);
        SubViewport.World2D = GetTree().Root.World2D;
    }

    public void Bind(GameScheduler gameScheduler)
    {
        _gameScheduler = gameScheduler;
        _gameScheduler.SimulationUpdated += GameSchedulerOnSimulationUpdated;
        _gameScheduler.OtherStateUpdated += GameSchedulerOnOtherStateUpdated;
        
        
        TreeExiting += () =>
        {
            _gameScheduler.SimulationUpdated -= GameSchedulerOnSimulationUpdated;
            _gameScheduler.OtherStateUpdated -= GameSchedulerOnOtherStateUpdated;
        };
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
    
    private void GameSchedulerOnOtherStateUpdated(object? sender, GameScheduler.OtherStateUpdateTypeEnum e)
    {
        switch (e)
        {
            case GameScheduler.OtherStateUpdateTypeEnum.FocusedUnitId:
                UnitsView.FocusUnit(_gameScheduler!.FocusedUnitId);
                SubCamera.Position = TilesView.GetRealPosition(
                    UnitsView.GetUnitView(_gameScheduler!.FocusedUnitId)?.WishCoords ?? _currentCoords, 
                    Layer.Underground);
                break;
            case GameScheduler.OtherStateUpdateTypeEnum.CoordsRegion:
                TilesView.DrawRegions(_gameScheduler!.CoordsRegions.Values,
                    _gameScheduler.GameState?.TileMapState.Size ?? Vector2I.Zero);
                break;
        }
    }

    private void GameSchedulerOnSimulationUpdated(object? sender, EventArgs e)
    {
        SetSimulations(_gameScheduler!.SimulationResults);
    }

    /// <param name="simulations">确保传入有序的模拟结果</param>
    private void SetSimulations(IEnumerable<SimulationResult> simulations)
    {
        if (IsRunning)
        {
            Logger.Info("缓存操作结果");
            _simulationCache.Enqueue(simulations.ToArray());
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
            await StartSimulations();
            RunCompleted();
        }
    }

    private void RunCompleted()
    {
        IsRunning = false;
        if (_simulationCache.TryDequeue(out var simulations))
        {
            SetSimulations(simulations);
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


    private async GDTask StartSimulations()
    {
        //绘制运行前的事件
        if (_runningSimulations.Count > 0)
        {
            _refocus = true;
            DrawEvents(_lastSimulations.Where(a => !_runningSimulations.ContainsKey(a.SourceId)));
        }
        await RunSimulations(_runningSimulations.Values);
        DrawEvents(_prepareSimulations);
    }

    private async GDTask RunSimulations(IEnumerable<SimulationResult> simulations)
    {
        foreach (var simulation in simulations)
        {
            // 清空当前事件视图
            //EventsView.ClearEventView(simulation.SourceId);
            if (simulation.Steps == null) continue;
            foreach (var step in simulation.Steps)
            {
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
                            var u = UnitFactory.CreateUnit(placeStep.UnitName, placeStep.Team, placeStep.Coords);
                            u.Id = placeStep.ResultUnit.Id;
                            await UnitsView.PlaceUnit(
                                placeStep.Coords,
                                u,
                                placeStep.Team.GetTeamTypeRelativeToPlayer(_gameScheduler!));
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
                            await UnitsView.MovePathUnit(movePathStep.ResultUnitId, movePathStep.Coords, movePathStep.Path, movePathStep.ResultTargetIndex);
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
                            UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView.AddVisibleBuff(modifyUnitBuffStep.BuffType);
                        }
                        else if (modifyUnitBuffStep.Operation == BuffOperation.RemoveBuff)
                        {
                            UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView.RemoveVisibleBuff(modifyUnitBuffStep.BuffType);
                        }
                        break;
                    case AnimationStep animationStep:
                        await AnimationView.Play(animationStep.Animation);
                        break;
                    case null:
                        break;
                }
            }
        }
    }

    private void DrawEvents(IEnumerable<SimulationResult> s)
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