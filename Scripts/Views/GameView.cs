using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class GameView : Node2D
{
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
    public GDTask RunSimulationTask { get; private set; } = GDTask.CompletedTask;
    public IPlayer? Player { get; set; }
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
        if (!RunSimulationTask.Status.IsCompleted()) return;
        switch (@event)
        {
            case InputEventMouseMotion e:
                _currentCoords = TilesView.GlobalMouseToTileCoords(GetGlobalMousePosition());
                if (_previousMousceCoords == null || _currentCoords != _previousMousceCoords)
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
            GD.Print("缓存操作结果");
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
    }
    private async GDTask RunSimulation()
    {

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
                        if (placeStep.ResultIsValid) await UnitsView.PlaceUnit(placeStep.Coords, placeStep.Unit);
                        break;
                    case MoveUnitStep moveStep:
                        UnitsView.CurrentUnitId = moveStep.ResultUnitId;
                        if (moveStep.ResultIsValid) await UnitsView.CurrentMoveUnit(moveStep.To);
                        UnitsView.UpdateUnitPosition(moveStep.To);
                        break;
                    case DamageStep damageStep:
                        UnitsView.CurrentUnitId = damageStep.ResultUnitId;
                        UnitsView.CurrentUnitHealth -= damageStep.ResultValidDamage;
                        break;
                    case MoveUnitPathStep movePathStep:
                        UnitsView.CurrentUnitId = movePathStep.ResultUnitId;
                        if (movePathStep.ResultIsValid) await UnitsView.CurrentMovePathUnit(movePathStep.Coords, movePathStep.Path, movePathStep.ResultTargetIndex);
                        break;
                }
            }
        }
        EventsView.ClearAll();
        foreach (var simulation in _prepareSimulations)
        {
            var isFocused = simulation.Layer == SimulationLayer.Focused;
            var view = EventsView.BeginAddEventView(simulation.SourceId);
            if (view == null) continue;
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
                        view.PlaceUnit(isFocused, placeStep.Coords, placeStep.Unit, placeStep.ResultIsValid);
                        break;
                    case MoveUnitStep moveStep:
                        view.MoveUnit(isFocused, moveStep.ResultUnitId, moveStep.Coords ,moveStep.To, moveStep.ResultIsValid);
                        break;
                    case DamageStep damageStep:
                        view.ChangeUnitHealth(isFocused, damageStep.ResultUnitId, -damageStep.ResultValidDamage);
                        break;
                    case MoveUnitPathStep movePathStep:
                        view.MoveUnitPath(isFocused, movePathStep.ResultUnitId, movePathStep.Coords, movePathStep.Path, movePathStep.ResultTargetIndex);
                        break;
                }
            }
            EventsView.EndAddEventView();
        }
    }
}