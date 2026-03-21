using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NLog;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class GameScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<GameScheduler> _runningSchedulers = new();
    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static GameScheduler? CurrentScheduler => _runningSchedulers.Peek();
    public IReadOnlyList<SchedulerEvent> WaitingSimulations => _waitingSimulations;
    public IReadOnlyList<SchedulerEvent> FocusedSimulations => _focusedSimulations;
    public IReadOnlyList<SchedulerEvent> RunningSimulations => _runningSimulations;
    public IReadOnlyList<SimulationResult> SimulationResults => _simulationResults;
    public IReadOnlyGameState? GameState => _gameState;
    public uint FocusedUnitId { get; private set; }
    public int PlayerTeam { get; private set; }

    public IReadOnlyDictionary<uint, (HashSet<Vector2I> Coords, int type)> CoordsRegions => _coordsRegions;

    private List<SchedulerEvent> _waitingSimulations = new();
    private List<SchedulerEvent> _focusedSimulations = new();
    private List<SchedulerEvent> _runningSimulations = new();
    private List<SimulationResult> _simulationResults = new();
    private List<SchedulerEvent>[] _simulationLayers;
    private int _blockingUpdate = 0;
    private List<SkillEvent> _skillEvents = new();
    private bool _needUpdate;
    private IGameState? _gameState;

    private Comparer<SchedulerEvent> _removeComparer =
        Comparer<SchedulerEvent>.Create(
            (a, b) => ReferenceEquals(a, b) ? 0 : a.Priority.CompareTo(b.Priority));
    private Dictionary<uint, (HashSet<Vector2I> Coords, int type)> _coordsRegions = new();
    private uint _coordsRegionsId;
    private IGameState? _snapshotChache;
    
    public event EventHandler? SimulationUpdated;
    public event EventHandler<OtherStateUpdateTypeEnum>? OtherStateUpdated;


    public GameScheduler()
    {
        _simulationLayers = [_runningSimulations, _focusedSimulations, _waitingSimulations];
    }

    public void InitScheduler(IGameState gameState, int playerTeam)
    { 
        _gameState = gameState;
        PlayerTeam = playerTeam;
    }
    public void FocusUnit(uint id)
    {
        if (FocusedUnitId == id) return;
        //调整对应技能层
        BlockUpdate();
        var skillEvent = _skillEvents.Find(e => e.BindUnitId == FocusedUnitId);
        (skillEvent as ISchedulerEvnet)?.Layer = SimulationLayer.Waiting;

        FocusedUnitId = id;
        
        //调整对应技能层
        skillEvent = id == 0 ? null : _skillEvents.Find(e => e.BindUnitId == FocusedUnitId);
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
    public SchedulerEvent AddEvent(SchedulerEvent ev, bool run = false)
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

    public void RemoveEvent(SchedulerEvent ev)
    {
        if (!RemoveEventList(ev)) return;
        ev.Update -= OnEventUpdate;
        
        //调整对应技能层
        if (ev is SkillEvent skillEvent)
        {
            _skillEvents.Remove(skillEvent);
            (skillEvent as ISchedulerEvnet)?.Layer = SimulationLayer.Waiting;
        }
        
        UpdateSimulation();
    }

    public uint AddCoordsRegion(IEnumerable<Vector2I> coords, int type)
    {
        var id = _coordsRegionsId++;
        _coordsRegions[id] = (coords.ToHashSet(), type);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
        return id;
    }
    public bool IsInCoordsRegion(uint id,in Vector2I coord) =>
        CoordsRegions.TryGetValue(id, out var region) && region.Coords.Contains(coord);
    public void RemoveCoordsRegion(uint id)
    {
        _coordsRegions.Remove(id);
        OtherStateUpdated?.Invoke(this, OtherStateUpdateTypeEnum.CoordsRegion);
    }

    private void AddEventList(SchedulerEvent ev)
    {
        var list = ev.Layer switch
        {
            SimulationLayer.Waiting => _waitingSimulations,
            SimulationLayer.Focused => _focusedSimulations,
            SimulationLayer.Running => _runningSimulations,
            _ => throw new ArgumentOutOfRangeException(nameof(ev.Layer), ev.Layer, null)
        };
        var index = list.BinarySearch(ev);
        index = index < 0 ? ~index : index;
        list.Insert(index, ev);
    }

    private void OnEventUpdate(object? sender, EventArgs e)
    {
        var ev = (sender as SchedulerEvent)!;
        RemoveEventList(ev);
        AddEventList(ev);
        UpdateSimulation();
    }

    private bool RemoveEventList(SchedulerEvent ev)
    {
        return _runningSimulations.Remove(ev) || _focusedSimulations.Remove(ev) || _waitingSimulations.Remove(ev);
    }
    private void UpdateSimulation()
    {
        if (_blockingUpdate > 0)
        {
            _needUpdate = true;
            return;
        }
        UpdateCore();
    }
    
    private void UpdateCore()
    {
        _simulationResults.Clear();
        if (_gameState is null) return;
        Logger.Info("触发更新");
        
        _runningSchedulers.Push(this);
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState) ? _gameState.GetSnapshot() : _snapshotChache;
        snapshot.ClearIfSnapshot();
        foreach (var layer in _simulationLayers)
        foreach (var ev in layer)
        {
            var result = new SimulationResult(
                ev.Id,
                GameState!.TeamState.GetTeamTypeRelativeToPlayer(PlayerTeam, ev.Team),
                ev.Layer);
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
            Logger.Debug("应用结果 {result}", a);
            _gameState.ApplyResult(a);
        }
        Logger.Debug("更新结果:\n{results}", string.Join("\n", SimulationResults));
        SimulationUpdated?.Invoke(this, EventArgs.Empty);
        
        _runningSchedulers.Pop();
    }

    private void BlockUpdate()
    {
        _blockingUpdate += 1;
        Logger.Trace("更新阻塞{count}", _blockingUpdate);
    }

    private void UnblockUpdate(bool endUpdate = true)
    {
        _blockingUpdate -= 1;
        Logger.Trace("更新解除阻塞{count}", _blockingUpdate);
        if (_blockingUpdate > 0 || !_needUpdate) return;
        if (endUpdate) UpdateSimulation();
        _needUpdate = false;
    }


    public enum OtherStateUpdateTypeEnum
    {
        None,
        FocusedUnitId,
        CoordsRegion,
    }
}