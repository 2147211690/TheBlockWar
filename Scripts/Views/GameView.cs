using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class GameView : Node2D
{
    private List<SimulationResult> _runningSimulations = new();
    private List<SimulationResult> _prepareSimulations = new();
    [Export] private TilesView _tilesView;
    [Export] private UnitsView _unitsView;
    [Export] private EventsView _eventsView;
    private GameScheduler? _gameScheduler;
    private Queue<SimulationResult[]> _simulationCache = new();
    public GDTask RunSimulationTask { get; private set; } = GDTask.CompletedTask;

    public void Bind(GameScheduler gameScheduler)
    {
        _gameScheduler = gameScheduler;
        _gameScheduler.SimulationUpdated += GameSchedulerOnSimulationUpdated;
        
        TreeExiting += () =>
        {
            _gameScheduler.SimulationUpdated -= GameSchedulerOnSimulationUpdated;
        };
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
            _eventsView.ClearEventView(simulation.SourceId);
            foreach (var step in simulation.Steps)
            {
                switch (step)
                {
                    case PlaceTileStep placeStep:
                        await _tilesView.PlaceTile(placeStep.Coords, placeStep.Tile);
                        _unitsView.UpdateUnitPosition(placeStep.Coords);
                        break;
                    case DropTileStep dropStep:
                        await _tilesView.DropTile(dropStep.Coords);
                        _unitsView.UpdateUnitPosition(dropStep.Coords);
                        break;
                    case PlaceUnitStep placeStep:
                        if (placeStep.ResultIsValid) await _unitsView.PlaceUnit(placeStep.Coords, placeStep.Unit);
                        break;
                    case MoveUnitStep moveStep:
                        _unitsView.CurrentUnitId = moveStep.ResultUnitId;
                        if (moveStep.ResultIsValid) await _unitsView.CurrentMoveUnit(moveStep.To);
                        _unitsView.UpdateUnitPosition(moveStep.To);
                        break;
                    case DamageStep damageStep:
                        _unitsView.CurrentUnitId = damageStep.ResultUnitId;
                        _unitsView.CurrentUnitHealth -= damageStep.ResultValidDamage;
                        break;
                }
            }
        }
        _eventsView.ClearAll();
        foreach (var simulation in _prepareSimulations)
        {
            var isFocused = simulation.Layer == SimulationLayer.Focused;
            var view = _eventsView.BeginAddEventView(simulation.SourceId);
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
                }
            }
            _eventsView.EndAddEventView();
        }
    }
}