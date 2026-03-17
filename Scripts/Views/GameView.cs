using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

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
    
    private List<SimulationResult> _runningSimulations = new();
    private List<SimulationResult> _prepareSimulations = new();
    private GameScheduler? _gameScheduler;
    private Queue<SimulationResult[]> _simulationCache = new();
    private Vector2I? _previousMousceCoords = null;
    private Vector2I _currentCoords;
    private bool _refocus = false;
    public GDTask RunSimulationTask { get; private set; } = GDTask.CompletedTask;
    public IPlayer? Player { get; set; }
    public TypedPool<AnimatedSprite2D> AnimatedSpritePool { get; private set; }
    public void Bind(GameScheduler gameScheduler)
    {
        _gameScheduler = gameScheduler;
        _gameScheduler.SimulationUpdated += GameSchedulerOnSimulationUpdated;
        _gameScheduler.OtherStateUpdated += GameSchedulerOnOtherStateUpdated;
        AnimatedSpritePool = new TypedPool<AnimatedSprite2D>(() =>
        {
            var node = new AnimatedSprite2D();
            DrawContainer.AddChild(node);
            return node;
        }, 0)
        {
            AfterPush = u =>
            {
                u.Visible = false;
            },
            BeforePop = u =>
            {
                u.Visible = true;
            }
        };
        TreeExiting += () =>
        {
            _gameScheduler.SimulationUpdated -= GameSchedulerOnSimulationUpdated;
            _gameScheduler.OtherStateUpdated -= GameSchedulerOnOtherStateUpdated;
            AnimatedSpritePool.Dispose();
        };
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!RunSimulationTask.Status.IsCompleted()) return;
        switch (@event)
        {
            case InputEventMouseMotion e:
                _currentCoords = TilesView.GlobalMouseToTileCoords(GetGlobalMousePosition());
                if (_previousMousceCoords is null || _currentCoords != _previousMousceCoords)
                {
                    _previousMousceCoords = _currentCoords;
                    Player?.OnInput(new PlayerInputMousceCoords(_currentCoords));
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
        if (!RunSimulationTask.Status.IsCompleted())
        {
            Logger.Info("缓存操作结果");
            _simulationCache.Enqueue(simulations.ToArray());
            return;
        }
        _prepareSimulations.Clear();
        _runningSimulations.Clear();
        foreach (var simulation in simulations)
        {
            if (simulation.Layer == SimulationLayer.Running)
                _runningSimulations.Add(simulation);
            else
                _prepareSimulations.Add(simulation);
        }
        RunSimulationTask = RunSimulation();
        RunSimulationTask.GetAwaiter().OnCompleted(RunCompleted);

    }
    private void RunCompleted()
    {
        if (_simulationCache.TryDequeue(out var simulations))
        {
            SetSimulations(simulations);
        }
        else if (_refocus) //如果完全完成了且运行过模拟,则重置下关注的坐标
        {
            Player?.OnInput(new PlayerInputMousceCoords(_currentCoords));
            _refocus = false;
        }
    }
    private async GDTask RunSimulation()
    {
        if (_runningSimulations.Count > 0) _refocus = true;
        foreach (var simulation in _runningSimulations)
        {
            // 清空当前事件视图
            EventsView.ClearEventView(simulation.SourceId);
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
                        //TODO: 处理单位buff变更
                        if (modifyUnitBuffStep.Operation == BuffOperation.AddBuff)
                        {
                            UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView.AddVisibleBuff(modifyUnitBuffStep.BuffType);
                        }
                        else if (modifyUnitBuffStep.Operation == BuffOperation.RemoveBuff)
                        {
                            UnitsView.GetUnitView(modifyUnitBuffStep.ResultUnitId)?.BuffView.RemoveVisibleBuff(modifyUnitBuffStep.BuffType);
                        }
                        break;
                }
            }
        }
        EventsView.ClearAll();
        foreach (var simulation in _prepareSimulations)
        {
            var isFocused = simulation.Layer == SimulationLayer.Focused;
            var view = EventsView.BeginAddEventView(simulation.SourceId);
            if (view is null) continue;
            foreach (var step in simulation.Steps)
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
                        view.PlaceUnit(isFocused, simulation.TeamType ,placeStep.Coords, placeStep.ResultUnit, placeStep.ResultIsValid);
                        break;
                    case MoveUnitStep moveStep:
                        view.MoveUnit(isFocused, moveStep.ResultUnitId, moveStep.Coords ,moveStep.To, moveStep.ResultIsValid);
                        break;
                    case DamageStep damageStep:
                        view.ChangeUnitHealth(isFocused, simulation.TeamType, damageStep.ResultUnitId, -damageStep.ResultValidDamage);
                        break;
                    case MoveUnitPathStep movePathStep:
                        view.MoveUnitPath(isFocused, simulation.TeamType, movePathStep.ResultUnitId, movePathStep.Coords, movePathStep.Path, movePathStep.ResultTargetIndex);
                        break;
                    case DropUnitStep dropStep:
                        view.DropUnit(isFocused, dropStep.Coords, dropStep.ResultIsValid);
                        break;
                    case PushUnitStep pushStep:
                        view.PushUnit(isFocused, simulation.TeamType, pushStep.PushDir, pushStep.Coords, pushStep.ResultPush);
                        break;
                    case ModifyUnitBuffStep modifyUnitBuffStep:
                        view.ModifyUnitBuff(isFocused, modifyUnitBuffStep.Coords,
                            modifyUnitBuffStep.ResultUnitId, modifyUnitBuffStep.BuffType,
                            modifyUnitBuffStep.ResultOperation, modifyUnitBuffStep.ResultDuration);
                        break;
                }
            }
            EventsView.EndAddEventView();
        }
    }
}