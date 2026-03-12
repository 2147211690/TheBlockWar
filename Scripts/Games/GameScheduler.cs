using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class GameScheduler
{
    public IReadOnlyList<SchedulerEvnet> WaitingSimulations => _waitingSimulations;
    public IReadOnlyList<SchedulerEvnet> FocusedSimulations => _focusedSimulations;
    public IReadOnlyList<SchedulerEvnet> RunningSimulations => _runningSimulations;
    public IReadOnlyList<SimulationResult> SimulationResults => _simulationResults;
    public IGameState? GameState { get; set; }
    
    
    private List<SchedulerEvnet> _waitingSimulations = new();
    private List<SchedulerEvnet> _focusedSimulations = new();
    private List<SchedulerEvnet> _runningSimulations = new();
    private List<SimulationResult> _simulationResults = new();
    private List<SchedulerEvnet>[] _simulationLayers;
    private int _blockingUpdate = 0;
    private List<SkillEvent> _skillEvents = new();
    private bool _needUpdate;

    private Comparer<SchedulerEvnet> _removeComparer =
        Comparer<SchedulerEvnet>.Create(
            (a, b) => ReferenceEquals(a, b) ? 0 : a.Priority.CompareTo(b.Priority));
    
    
    public event EventHandler? SimulationUpdated;
    public event EventHandler<OtherStateUpdateTypeEnum>? OtherStateUpdated;


    public GameScheduler()
    {
        _simulationLayers = [_runningSimulations, _focusedSimulations, _waitingSimulations];
    }


    public void FocusUnit(uint id)
    {
        //if (GameState?.FocusedUnitId == id) return;
        //调整对应技能层
        BlockUpdate();
        var skillEvent = _skillEvents.Find(e => e.BindUnitId == GameState?.FocusedUnitId);
        skillEvent?.Layer = SimulationLayer.Waiting;
        
        GameState?.FocusedUnitId = id;
        
        //调整对应技能层
        skillEvent = _skillEvents.Find(e => e.BindUnitId == GameState?.FocusedUnitId);
        skillEvent?.Layer = SimulationLayer.Focused;
        UnblockUpdate();
        
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.FocusedUnitId);
    }

    public void FocusCoords(Vector2I coords)
    {
        var unit = GameState?.GetUnit(coords);
        FocusUnit(unit?.Id ?? 0);
    }

    public SchedulerEvnet AddEvent(SchedulerEvnet ev)
    {
        //调整对应技能层
        if (ev is SkillEvent skillEvent)
        {
            _skillEvents.Add(skillEvent);
            if (GameState?.FocusedUnitId == skillEvent.BindUnitId) skillEvent.Layer = SimulationLayer.Focused;
        }
        
        AddEventList(ev);
        ev.Update += OnEventUpdate;
        UpdateSimulation();
        return ev;
    }

    public void RemoveEvent(SchedulerEvnet ev)
    {
        //调整对应技能层
        if (ev is SkillEvent skillEvent)
        {
            _skillEvents.Remove(skillEvent);
            skillEvent.Layer = SimulationLayer.Waiting;
        }
        RemoveEventList(ev);
        ev.Update -= OnEventUpdate;
        UpdateSimulation();
    }


    private void AddEventList(SchedulerEvnet ev)
    {
        var list = ev.Layer switch
        {
            SimulationLayer.Waiting => _waitingSimulations,
            SimulationLayer.Focused => _focusedSimulations,
            SimulationLayer.Running => _runningSimulations,
            _ => throw new ArgumentOutOfRangeException(nameof(ev.Layer), ev.Layer, null)
        };
        var index = list.BinarySearch(ev);
        list.Insert(index < 0 ? ~index : index, ev);
        /*list.Add(ev);
        list.Sort();*/
    }

    private void OnEventUpdate(object? sender, EventArgs e)
    {
        var ev = (sender as SchedulerEvnet)!;
        RemoveEventList(ev);
        AddEventList(ev);
        UpdateSimulation();
    }

    private void RemoveEventList(SchedulerEvnet ev)
    {
        if (TryRemoveFromList(_runningSimulations, ev)) return;
        if (TryRemoveFromList(_focusedSimulations, ev)) return;
        if (TryRemoveFromList(_waitingSimulations, ev)) return;
        //if (_runningSimulations.Remove(ev) || _focusedSimulations.Remove(ev) || _waitingSimulations.Remove(ev)) ;
    }

    private bool TryRemoveFromList(List<SchedulerEvnet> list, SchedulerEvnet ev)
    {
        if (list.Count == 0) return false;
        var index = list.BinarySearch(0, list.Count, ev, _removeComparer);
        if (index < 0) return false;
        for (var i = index; i >= 0 && list[i].CompareTo(ev) == 0 ; i--)
        {
            if (!ReferenceEquals(list[i], ev)) continue;
            list.RemoveAt(i);
            return true;
        }
        for (var i = index + 1; i < list.Count && list[i].CompareTo(ev) == 0; i++)
        {
            if (!ReferenceEquals(list[i], ev)) continue;
            list.RemoveAt(i);
            return true;
        }
        return false;
    }

    private void UpdateSimulation()
    {
        if (_blockingUpdate > 0)
        {
            _needUpdate = true;
            return;
        }
        GD.Print("触发更新");
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

    private void BlockUpdate()
    {
        _blockingUpdate += 1;
    }

    private void UnblockUpdate()
    {
        _blockingUpdate -= 1;
        if (_blockingUpdate > 0 || !_needUpdate) return;
        UpdateSimulation();
        _needUpdate = false;
    }


    public enum OtherStateUpdateTypeEnum
    {
        None,
        FocusedUnitId,
    }
}