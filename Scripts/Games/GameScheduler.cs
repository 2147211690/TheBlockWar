using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NLog;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class GameScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public IReadOnlyList<SchedulerEvnet> WaitingSimulations => _waitingSimulations;
    public IReadOnlyList<SchedulerEvnet> FocusedSimulations => _focusedSimulations;
    public IReadOnlyList<SchedulerEvnet> RunningSimulations => _runningSimulations;
    public IReadOnlyList<SimulationResult> SimulationResults => _simulationResults;
    public IReadOnlyGameState? GameState => _gameState;
    public uint FocusedUnitId { get; private set; }
    
    private List<SchedulerEvnet> _waitingSimulations = new();
    private List<SchedulerEvnet> _focusedSimulations = new();
    private List<SchedulerEvnet> _runningSimulations = new();
    private List<SimulationResult> _simulationResults = new();
    private List<SchedulerEvnet>[] _simulationLayers;
    private int _blockingUpdate = 0;
    private List<SkillEvent> _skillEvents = new();
    private bool _needUpdate;
    private IGameState? _gameState;

    private Comparer<SchedulerEvnet> _removeComparer =
        Comparer<SchedulerEvnet>.Create(
            (a, b) => ReferenceEquals(a, b) ? 0 : a.Priority.CompareTo(b.Priority));
    private Dictionary<uint, (Vector2I[] Coords, int type)> _coordsRegions = new();
    private uint _coordsRegionsId;
    
    public event EventHandler? SimulationUpdated;
    public event EventHandler<OtherStateUpdateTypeEnum>? OtherStateUpdated;


    public GameScheduler()
    {
        _simulationLayers = [_runningSimulations, _focusedSimulations, _waitingSimulations];
    }

    public void SetGameState(IGameState? gameState) => this._gameState = gameState;
    public void FocusUnit(uint id)
    {
        if (FocusedUnitId == id) return;
        //调整对应技能层
        BlockUpdate();
        var skillEvent = _skillEvents.Find(e => e.BindUnitId == FocusedUnitId);
        (skillEvent as ISchedulerEvnet)?.Layer = SimulationLayer.Waiting;

        FocusedUnitId = id;
        
        //调整对应技能层
        skillEvent = _skillEvents.Find(e => e.BindUnitId == FocusedUnitId);
        (skillEvent as ISchedulerEvnet)?.Layer = SimulationLayer.Focused;
        UnblockUpdate();
        
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.FocusedUnitId);
    }

    public uint FocusCoords(Vector2I coords)
    {
        var unit = _gameState?.UnitMapState.GetUnit(coords);
        var id = unit?.Id ?? 0;
        FocusUnit(id);
        return id;
    }

    public SkillEvent? GetUnitSkillEvent(uint unitId)
    {
        return _skillEvents.Find(e => e.BindUnitId == unitId);
    }
    public SchedulerEvnet AddEvent(SchedulerEvnet ev, bool run = false)
    {
        if (run) (ev as ISchedulerEvnet)?.Layer = SimulationLayer.Running;
        //调整对应技能层
        if (ev is SkillEvent skillEvent)
        {
            Logger.Debug("添加技能{event}", skillEvent);
            _skillEvents.Add(skillEvent);
            if (FocusedUnitId == skillEvent.BindUnitId)
                (skillEvent as ISchedulerEvnet).Layer = SimulationLayer.Focused;
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
            (skillEvent as ISchedulerEvnet)?.Layer = SimulationLayer.Waiting;
        }
        RemoveEventList(ev);
        ev.Update -= OnEventUpdate;
        UpdateSimulation();
    }

    public uint AddCoordsRegion(IEnumerable<Vector2I> coords, int type)
    {
        var id = _coordsRegionsId++;
        _coordsRegions[id] = (coords.ToArray(), type);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
        return id;
    }

    public void RemoveCoordsRegion(uint id)
    {
        _coordsRegions.Remove(id);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
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

        Logger.Info("触发更新");
        _simulationResults.Clear();
        if (_gameState is null) return;
        var snapshot = _gameState.GetSnapshot();
        foreach (var layer in _simulationLayers)
        foreach (var ev in layer)
        {
            var result = new SimulationResult(ev.Id) { Layer = ev.Layer };
            ev.Action?.Invoke(snapshot, ev.Args, result);
            _simulationResults.Add(result);
        }
        // 运行层事件执行完毕后，清空运行层
        foreach (var ev in _runningSimulations)
        {
            if (ev is SkillEvent skillEvent) _skillEvents.Remove(skillEvent);
            ev.Update -= OnEventUpdate;
        }
        _runningSimulations.Clear();
        
        // 应用运行层事件结果
        foreach (var a in SimulationResults)
        {
            if (a.Layer != SimulationLayer.Running) break;
            Logger.Info("应用结果 {result}", a);
            _gameState.ApplyResult(a);
        }
        Logger.Info("更新结果:\n{results}", string.Join("\n", SimulationResults));
        SimulationUpdated?.Invoke(this, EventArgs.Empty);
    }

    private void BlockUpdate()
    {
        _blockingUpdate += 1;
        Logger.Trace("更新阻塞{count}", _blockingUpdate);
    }

    private void UnblockUpdate()
    {
        _blockingUpdate -= 1;
        Logger.Trace("更新解除阻塞{count}", _blockingUpdate);
        if (_blockingUpdate > 0 || !_needUpdate) return;
        UpdateSimulation();
        _needUpdate = false;
    }


    public enum OtherStateUpdateTypeEnum
    {
        None,
        FocusedUnitId,
        CoordsRegion,
    }
}