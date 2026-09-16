using System.Linq;
using System.Text;
using System.Text.Json;
using NLog;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Tools.Jsons;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 控制游戏的总调度器,请在任何与游戏状态相关的操作中使用.
/// </summary>
public class EventScheduler : IEventScheduler
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private static Stack<IEventScheduler> _runningSchedulers = new();

    /// <summary>
    /// 当前正在运行的调度器,如果没有正在运行的调度器,则为null
    /// 注意不要在模拟时更新调度器内容,仅用来查询当前游戏状态
    /// </summary>
    public static IEventScheduler CurrentScheduler => _runningSchedulers.Peek();
    
    public IReadOnlyGameState GameState => _gameState;
    public int PlayerTeam { get; }
    public int CurrentTurn { get => _gameState.TurnState.CurrentTurn; set => _gameState.TurnState.CurrentTurn = value; }
    private IGameState _gameState;
    private IGameState? _snapshotChache;
    public event EventHandler<SchedulerResult>? SimulationUpdated;
    public event EventHandler<IReadOnlyGameState>? GameStateReload;

    public Dictionary<int, Dictionary<string, int>> PhaseOrders => _gameState.TurnState.PhaseOrders;

    public bool IsRunning
    {
        get;
        set
        { 
            if (field == value) return;
            field = value;
            if (field)
            {
                _runningCompletionSource = new();
                _runningCompletionSource.Task.Forget();
            }
            else
            {
                _runningCompletionSource!.TrySetResult();
                _runningCompletionSource = null;
            }
        }
    }
    private GDTaskCompletionSource? _runningCompletionSource;

    private bool _isSimulating = false;
    private IEventScheduler _eventSchedulerImplementation;
    private int _updateCount = 0;
    private int _blockCount = 0;

    /// <summary>
    /// 当前关注的目标单位(悬停的单位)。
    /// </summary>
    public long? FocusedUnitId
    {
        get => _focusedUnitId;
        set
        {
            if (_focusedUnitId == value) return;
            _focusedUnitId = value;
            UpdateSimulation();
        }
    }

    /// <summary>
    /// 当前关注的目标坐标(悬停的格子)。
    /// </summary>
    public Vector2I? FocusedCoords
    {
        get => _focusedCoords;
        set
        {
            if (_focusedCoords == value) return;
            _focusedCoords = value;
            UpdateSimulation();
        }
    }
    private long? _focusedUnitId;
    private Vector2I? _focusedCoords;
    private bool _isFocusedRegionTurn;
    private bool _isFocusedBuffTurn;

    /// <summary>
    /// 布尔聚焦标记:为 true 时,收集器把对应的整类触发者标记为 Focus(仅预览),否则 Running。
    /// 变化时触发一次全量更新。
    /// </summary>
    public bool IsFocusedRegionTurn
    {
        get => _isFocusedRegionTurn;
        set
        {
            if (_isFocusedRegionTurn == value) return;
            _isFocusedRegionTurn = value;
            UpdateSimulation();
        }
    }

    public bool IsFocusedBuffTurn
    {
        get => _isFocusedBuffTurn;
        set
        {
            if (_isFocusedBuffTurn == value) return;
            _isFocusedBuffTurn = value;
            UpdateSimulation();
        }
    }

    public bool IsFocusedSkillTurn
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateSimulation();
        }
    }

    /// <summary>
    /// 最近一次更新收集到的"下回合触发者"列表。
    /// </summary>
    public IReadOnlyList<TurnTrigger> NextTurnTriggers { get; private set; } = [];

    /// <summary>
    /// 统一阶段事件查询:返回指定阶段(phaseOrder)的"下回合触发者"(区域/单位buff/单位技能)。
    /// 只从最近一次(已全量收集的) NextTurnTriggers 中过滤,不触发新一轮收集;
    /// Running 真实回合执行尚未实现。
    /// </summary>
    public IReadOnlyList<TurnTrigger> QueryPhaseTriggers(int phaseOrder)
        => NextTurnTriggers.Where(t => t.PhaseOrder == phaseOrder).ToList();

    public EventScheduler(IGameState gameState, int playerTeam)
    {
        _gameState = gameState;
        PlayerTeam = playerTeam;
        _gameState.EventManagerState.EnabledEventUpdated += OnEnabledEventUpdated;
    }

    private void OnEnabledEventUpdated(object? sender, EventArgs e)
    {
        UpdateSimulation();
    }

    public void ReloadGameState()
    {
        GameStateReload?.Invoke(this, GameState);
        UpdateSimulation();
    }

    public void BlockUpdate()
    {
        _blockCount++;
    }

    public void UnblockUpdate()
    {
        _blockCount--;
        if (_blockCount <= 0 && _updateCount > 0)
        {
            UpdateSimulation();
        }
    }

    public GDTask WaitCompleteRunning() => _runningCompletionSource?.Task ?? GDTask.CompletedTask;
    public IReadOnlyDictionary<long, EventState> Events => _gameState.EventManagerState.Events;
    public IReadOnlyList<EventState> GetCoordsEvents(Vector2I coords)
        => _gameState.EventManagerState.GetCoordsEvents(coords);
    public IReadOnlyList<EventState> GetUnitEvents(long unitId)
        => _gameState.EventManagerState.GetUnitEvents(unitId);
    public IReadOnlyList<EventState> GetTeamEvents(int team)
        => _gameState.EventManagerState.GetTeamEvents(team);
    public EventState CreateEvent(EventCore core,
        int phaseOrder, int team,
        SimulationLayer layer,
        int priority = 0,
        EventAfterRunMode afterRunMode = EventAfterRunMode.Remove)
        => _gameState.CreateEvent(core,phaseOrder, team, layer, priority, afterRunMode);
    public void RemoveEvent(long id)
        => _gameState.EventManagerState.RemoveEvent(id);

    /// <summary>
    /// 设置单位下回合准备使用的技能；会触发一次更新。
    /// </summary>
    public void SetUnitPrepareSkill(long unitId, PrepareSkillInfo? skill)
    {
        _gameState.UnitMapState.ModifyUnit(unitId, u => u.PrepareSkill = skill);
        UpdateSimulation();
    }

    /// <summary>
    /// 在快照上执行单位下回合准备使用的技能,重放到真数据,清理该单位的 Prepare,并存入 result;随后走一次更新。
    /// 该次更新的 result 与本次技能执行共用同一个对象。
    /// </summary>
    public void RunUnitPrepareSkill(long unitId, float focusTime = 0.5f, float afterTime = 0.5f)
    {
        var result = new SchedulerResult()
        {
            DelayTime = afterTime
        };
        _focusedUnitId = unitId;
        DoUpdate(new SchedulerResult()
        {
            DelayTime = focusTime
        }, null);
        DoUpdate(result, res =>
        {
            var unit = _gameState.UnitMapState.GetUnit(unitId);
            if (unit.IsDead || unit.PrepareSkill is not { } skill) return;
            // 直接在真数据上运行技能(真实执行,不需要再 Replay)
            var sim = new SimObject(_gameState, SimulationLayer.Running, new(), 0);
            new SkillTurnTrigger().Init(
                SimulationLayer.Running, 0, 0, unitId, skill.SkillName, skill.Args).Invoke(sim);
            res.RunningCount++;
            res.Sims.Add(sim);
        });
    }

    /// <summary>
    /// 在快照上执行所有Buff的下回合(执行其 OnEnterTurn 钩子),存入 result 并走一次更新。
    /// 仿照 <see cref="RunUnitPrepareSkill"/>:在真数据上 Running 执行,不做 Replay。
    /// </summary>
    public void RunBuffTurn(float delayTime = 0.5f)
    {
        var result = new SchedulerResult();
        _isFocusedBuffTurn = true;
        DoUpdate(new SchedulerResult() { DelayTime = delayTime }, null);
        _isFocusedBuffTurn = false;
        DoUpdate(result, res =>
        {
            var buffs = _gameState.CollectBuffTurnTriggers(true);
            foreach (var buff in buffs)
            {
                var sim = new SimObject(_gameState, SimulationLayer.Running, new(), 0);
                if (!buff.CheckIsValid(sim)) continue;
                buff.Invoke(sim);
                res.RunningCount++;
                res.Sims.Add(sim);
            }
            TurnTriggerCollector.ReturnTriggers(buffs);
        });
    }

    /// <summary>
    /// 在快照上执行所有区域的下回合(执行其 OnEnterTurn 钩子),存入 result 并走一次更新。
    /// 仿照 <see cref="RunUnitPrepareSkill"/>:在真数据上 Running 执行,不做 Replay。
    /// </summary>
    public void RunRegionTurn(float delayTime = 0.5f)
    {
        var result = new SchedulerResult();
        _isFocusedRegionTurn = true;
        DoUpdate(new SchedulerResult() { DelayTime = delayTime }, null);
        _isFocusedRegionTurn = false;
        DoUpdate(result, res =>
        {
            var regions = _gameState.CollectRegionTurnTriggers(true);
            foreach (var region in regions)
            {
                var sim = new SimObject(_gameState, SimulationLayer.Running, new(), 0);
                if (!region.CheckIsValid(sim)) continue;
                region.Invoke(sim);
                res.RunningCount++;
                res.Sims.Add(sim);
            }
            TurnTriggerCollector.ReturnTriggers(regions);
        });
    }


    /// <summary>
    /// 在快照上执行某队伍所有单位下回合准备使用的技能,存入 result 并走一次更新。
    /// 仿照 <see cref="RunBuffTurn"/>:先做一次整类聚焦预览,再在真数据上 Running 执行全部技能。
    /// </summary>
    public void RunTeamUnitSkillTurn(int team, float delayTime = 0.5f)
    {
        var result = new SchedulerResult();
        var skills = _gameState.CollectSkillTurnTriggers(true, null, team);
        foreach (var skill in skills)
        {
            RunUnitPrepareSkill(skill.UnitId, 0.5f, 0.5f);
        }
        TurnTriggerCollector.ReturnTriggers(skills);
    }
    
    
    

    /// <summary>
    /// 直接在真数据上执行一次性动作,存入 result 并走一次更新。
    /// 仿照 <see cref="RunUnitPrepareSkill"/>:用 Running 层的 SimObject 在真数据上执行,不做 Replay。
    /// </summary>
    public void RunAction(Action<SimObject> action)
    {
        var result = new SchedulerResult();
        DoUpdate(result, res =>
        {
            if (action is null) return;
            var sim = new SimObject(_gameState, SimulationLayer.Running, new(), 0);
            action(sim);
            res.RunningCount++;
            res.Sims.Add(sim);
        });
    }

    /// <summary>
    /// 游戏状态相关更新的总入口。主循环:创建快照后收集并执行"下回合触发者"。
    /// </summary>
    public void UpdateSimulation() => DoUpdate(new SchedulerResult(), null);

    private void DoUpdate(SchedulerResult result, Action<SchedulerResult>? extra)
    {
        if (_isSimulating) return;
        if (_blockCount > 0)
        {
            _updateCount++;
            return;
        }
        
        _isSimulating = true;
        _runningSchedulers.Push(this);

        Logger.Info("触发更新");
        
        extra?.Invoke(result);
        
        // 创建快照，收集只读，不修改游戏状态
        var snapshot = _snapshotChache =
            (_snapshotChache is null || _snapshotChache.Owner != GameState)
                ? _gameState.GetSnapshot()
                : _snapshotChache;
        snapshot.ClearIfSnapshot();

        // 新主循环：先执行额外动作(如执行并重放单位技能),再收集"下回合触发者"。
        // 这样已执行的技能因 PrepareSkill 被清理,不会再被收集成一次额外预览。
        var obj = new SimObject(snapshot, SimulationLayer.Focused, new(), 0);


        NextTurnTriggers = snapshot.CollectNextTurnTriggers(FocusedCoords, FocusedUnitId,
            IsFocusedRegionTurn, IsFocusedBuffTurn, IsFocusedSkillTurn);

        // 开始模拟：执行每个有效触发者,把产生的步骤存入 result
        foreach (var trigger in NextTurnTriggers)
        {
            if (!trigger.CheckIsValid(obj)) continue; // 失效舍弃
            var sim = new SimObject(snapshot, trigger.Layer, new(), 0);
            trigger.Invoke(sim);
            result.RunningCount += sim.Layer == SimulationLayer.Running ? 1 : 0;
            result.Sims.Add(sim);
        }
        
        // 使用完主动归还,复用池对象(不归还也不会泄漏:池空时下次 Get 会新建)
        TurnTriggerCollector.ReturnTriggers(NextTurnTriggers);

        // 通知外部更新完成
        SimulationUpdated?.Invoke(this, result);

        _runningSchedulers.Pop();
        _isSimulating = false;
        _blockCount = 0;
        _updateCount = 0;
    }
}