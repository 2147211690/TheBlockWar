using System;
using System.Collections.Generic;
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
                }
            }
            _eventsView.EndAddEventView();
        }
    }
}