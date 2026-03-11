using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public class GameScheduler
{
    public IReadOnlyList<SchedulerEvnet> WaitingSimulations => _waitingSimulations;
    public IReadOnlyList<SchedulerEvnet> FocusedSimulations => _focusedSimulations;
    public IReadOnlyList<SchedulerEvnet> RunningSimulations => _runningSimulations;
    public IReadOnlyList<SimulationResult> SimulationResults => _simulationResults;

    private List<SchedulerEvnet> _waitingSimulations = new();
    private List<SchedulerEvnet> _focusedSimulations = new();
    private List<SchedulerEvnet> _runningSimulations = new();
    private List<SimulationResult> _simulationResults = new();
    private List<SchedulerEvnet>[] _simulationLayers;
    
    private bool _allowUpdate = true;
    public event EventHandler? SimulationUpdated;
    public IGameState? GameState { get; set; }

    public GameScheduler()
    {
        _simulationLayers = [_runningSimulations, _focusedSimulations, _waitingSimulations];
    }

    public SchedulerEvnet AddEvent(SchedulerEvnet ev)
    {
        var list = ev.Layer switch
        {
            SimulationLayer.Waiting => _waitingSimulations,
            SimulationLayer.Focused => _focusedSimulations,
            SimulationLayer.Running => _runningSimulations,
            _ => throw new ArgumentOutOfRangeException(nameof(ev.Layer), ev.Layer, null)
        };
        list.Add(ev);
        //TODO:有序插入
        list.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        ev.Update += OnEventUpdate;
        UpdateSimulation();
        return ev;
    }

    private void OnEventUpdate(object? sender, EventArgs e)
    {
        var ev = (sender as SchedulerEvnet)!;
        _allowUpdate = false;
        RemoveEvent(ev);
        AddEvent(ev);
        _allowUpdate = true;
        UpdateSimulation();
    }

    public void RemoveEvent(SchedulerEvnet ev)
    {
        //TODO:有序删除
        if (_runningSimulations.Remove(ev) || _focusedSimulations.Remove(ev) || _waitingSimulations.Remove(ev)) ;
        ev.Update -= OnEventUpdate;
        UpdateSimulation();
    }

    private void UpdateSimulation()
    {
        if (!_allowUpdate) return;
        _simulationResults.Clear();
        if (GameState is null) return;
        var snapshot = GameState.GetSnapshot();
        foreach (var layer in _simulationLayers)
        foreach (var ev in layer)
        {
            var result = new SimulationResult(ev.Id) { Layer = ev.Layer };
            ev.Action?.Invoke(snapshot, ev.Args, result);
            _simulationResults.Add(result);
        }
        // 运行层事件执行完毕后，清空运行层
        foreach (var ev in _runningSimulations) ev.Update -= OnEventUpdate;
        _runningSimulations.Clear();
        
        // 应用运行层事件结果
        foreach (var a in SimulationResults)
        {
            if (a.Layer != SimulationLayer.Running) break;
            GameState.ApplyResult(a);
        }
        
        SimulationUpdated?.Invoke(this, EventArgs.Empty);
    }
}