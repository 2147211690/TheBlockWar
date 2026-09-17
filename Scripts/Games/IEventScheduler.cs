using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Games.Turns.Phases;

namespace TheBlockWar.Scripts.Games;

public interface IEventScheduler
{
    IReadOnlyGameState GameState { get; }
    int PlayerTeam { get; }
    int CurrentTurn { get; set; }
    Dictionary<int, Dictionary<string, int>> PhaseOrders { get; }
    bool IsRunning { get; set; }
    event EventHandler<SchedulerResult>? SimulationUpdated;
    event EventHandler<IReadOnlyGameState>? GameStateReload;
    void ReloadGameState();
    void BlockUpdate();
    void UnblockUpdate();
    void SetNextTurnPhaseOrder(IEnumerable<ITurnPhase> phases);
    void SetCurrentTurnPhaseOrder(IEnumerable<ITurnPhase> phases);
    GDTask WaitCompleteRunning();

    /// <summary>
    /// 当前关注的目标单位(悬停的单位)。变化时触发一次更新。
    /// </summary>
    long? FocusedUnitId { get; set; }
    /// <summary>
    /// 当前关注的目标坐标(悬停的格子)。变化时触发一次更新。
    /// </summary>
    Vector2I? FocusedCoords { get; set; }

    /// <summary>
    /// 布尔聚焦标记:为 true 时,收集器把对应的整类触发者标记为 Focus(仅预览),否则 Running。
    /// 变化时触发一次全量更新。
    /// </summary>
    bool IsFocusedRegionTurn { get; set; }
    bool IsFocusedBuffTurn { get; set; }
    bool IsFocusedSkillTurn { get; set; }

    /// <summary>
    /// 设置单位下回合准备使用的技能；会触发一次更新。
    /// </summary>
    void SetUnitPrepareSkill(long unitId, PrepareSkillInfo? skill);

    /// <summary>
    /// 在快照上执行单位下回合准备使用的技能,重放到真数据,清理该单位的 Prepare,并存入 result;随后走一次更新。
    /// </summary>
    void RunUnitPrepareSkill(long unitId, float focusTime = 0.5f, float afterTime = 0.5f);

    /// <summary>
    /// 在快照上执行所有Buff的下回合(执行其 OnEnterTurn 钩子),存入 result 并走一次更新。
    /// </summary>
    void RunBuffTurn(float delayTime = 0.5f);

    /// <summary>
    /// 在快照上执行所有区域的下回合(执行其 OnEnterTurn 钩子),存入 result 并走一次更新。
    /// </summary>
    void RunRegionTurn(float delayTime = 0.5f);

    /// <summary>
    /// 在快照上执行某队伍所有单位下回合准备使用的技能,存入 result 并走一次更新。
    /// </summary>
    void RunTeamUnitSkillTurn(int team, float delayTime = 0.5f);

    /// <summary>
    /// 直接在真数据上执行一次性动作,存入 result 并走一次更新。
    /// </summary>
    void RunAction(Action<SimObject> action);
    
}