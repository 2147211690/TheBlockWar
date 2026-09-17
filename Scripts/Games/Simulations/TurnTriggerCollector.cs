using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

/// <summary>
/// 一个"下回合触发者"：表示某实体（区域 / 单位Buff / 单位技能）在下回合将执行一次回合钩子。
/// 子类各自承载执行与有效性校验，避免 lambda 闭包捕获过时状态。
/// 实例通过 <see cref="TypedPool{T}"/> 池化复用：收集器 Get + Init，下次收集前统一 Return。
/// </summary>
public abstract class TurnTrigger
{
    public SimulationLayer Layer { get; set; }
    public int PhaseOrder { get; private set; }
    public int Priority { get; private set; }
    public string Description { get; private set; } = string.Empty;

    protected TurnTrigger() { }

    /// <summary>重置并填充基础字段(供池化复用)。</summary>
    public void Init(SimulationLayer layer, int phaseOrder, int priority, string description)
    {
        Layer = layer;
        PhaseOrder = phaseOrder;
        Priority = priority;
        Description = description;
    }

    /// <summary>清空引用,准备还回池。</summary>
    public virtual void Reset() { }

    /// <summary>执行前校验是否仍有效；失效的触发者应被舍弃。</summary>
    public abstract bool CheckIsValid(SimObject obj);

    /// <summary>执行本触发者的回合钩子（此时已处于正确 Layer）。</summary>
    public abstract void Invoke(SimObject obj);
}

/// <summary>区域触发者：格子上一块区域在下回合执行 OnEnterTurn。</summary>
public sealed class RegionTurnTrigger : TurnTrigger
{
    private Vector2I _coords;
    private string _regionName = string.Empty;
    private string _regionData = string.Empty;
    private int _turn;
    private IRegion? _hook;

    public RegionTurnTrigger Init(SimulationLayer layer, int phaseOrder, int priority,
        Vector2I coords, string regionName, string regionData, int turn, IRegion hook)
    {
        base.Init(layer, phaseOrder, priority, $"{regionName}@{coords}");
        _coords = coords;
        _regionName = regionName;
        _regionData = regionData;
        _turn = turn;
        _hook = hook;
        return this;
    }

    public override void Reset()
    {
        base.Reset();
        _regionName = string.Empty;
        _regionData = string.Empty;
        _turn = 0;
        _hook = null;
    }

    public override bool CheckIsValid(SimObject obj)
    {
        return obj.State.TileMapState.GetTile(_coords).FindRegion(_regionName) >= 0;
    }

    public override void Invoke(SimObject obj)
    {
        _hook!.OnEnterTurn(obj, _coords, _regionData, _turn);
        // 调用后删减回合层数:仿 Simulator.ApplyModifyTileRegion 的 RemoveRegion,每回合扣1,归0自动移除
        obj.ApplyModifyTileRegion(_coords, _regionName, RegionOperation.RemoveRegion, 1, null, false);
    }
}

/// <summary>Buff触发者：单位身上的一个Buff在下回合执行 OnEnterTurn。</summary>
public sealed class BuffTurnTrigger : TurnTrigger
{
    private long _unitId;
    private string _buffName = string.Empty;
    private int _count;
    private int _turn;
    private IBuff? _hook;

    public BuffTurnTrigger Init(SimulationLayer layer, int phaseOrder, int priority,
        long unitId, string buffName, int count, int turn, IBuff hook)
    {
        base.Init(layer, phaseOrder, priority, $"{buffName}@{unitId}");
        _unitId = unitId;
        _buffName = buffName;
        _count = count;
        _turn = turn;
        _hook = hook;
        return this;
    }

    public override void Reset()
    {
        base.Reset();
        _unitId = 0;
        _buffName = string.Empty;
        _count = 0;
        _turn = 0;
        _hook = null;
    }

    public override bool CheckIsValid(SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(_unitId);
        if (unit.IsDead) return false;
        return unit.FindBuff(_buffName) >= 0;
    }

    public override void Invoke(SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(_unitId);
        if (unit.IsDead) return;
        _hook!.OnEnterTurn(obj, unit, _count, _turn);
        // 调用后删减回合层数:仿 Simulator.ApplyBuffTurn,按 TurnRemoveCount 移除;0/∞ 不减,TurnRemoveCount==0 不减
        if (_count is 0 or int.MaxValue) return;
        var info = _buffName.GetBuffInfo();
        if (info.TurnRemoveCount == 0) return;
        obj.ApplyModifyUnitBuff(unit.Coords, _buffName, BuffOperation.RemoveBuff, info.TurnRemoveCount, false);
    }
}

/// <summary>技能触发者：单位下回合准备使用的技能。</summary>
public sealed class SkillTurnTrigger : TurnTrigger
{
    private long _unitId;
    /// <summary>对应的单位Id。</summary>
    public long UnitId => _unitId;
    public int UnitTeam { get; private set; }
    private string _skillName = string.Empty;
    private Vector2I?[]? _args;

    public SkillTurnTrigger Init(SimulationLayer layer, int phaseOrder, int priority,
        long unitId, int unitTeam, string skillName, Vector2I?[]? args)
    {
        base.Init(layer, phaseOrder, priority, $"技能:{skillName}@{unitId}");
        UnitTeam = unitTeam;
        _unitId = unitId;
        _skillName = skillName;
        _args = args;
        return this;
    }

    public override void Reset()
    {
        base.Reset();
        _unitId = 0;
        _skillName = string.Empty;
        _args = null;
    }

    public override bool CheckIsValid(SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(_unitId);
        return !unit.IsDead && unit.PrepareSkill?.SkillName == _skillName;
    }

    public override void Invoke(SimObject obj)
    {
        var unit = obj.State.UnitMapState.GetUnit(_unitId);
        if (unit.IsDead) return;
        obj.ApplyUnitSkill(unit.Id, _skillName, unit.Coords, _args);
    }
}

public static class TurnTriggerCollector
{
    /// <summary>各触发者的对象池;AfterReturn 时清空引用以安全复用。</summary>
    private static readonly TypedPool<RegionTurnTrigger> RegionTriggerPool = new(
        () => new RegionTurnTrigger(), 16, afterReturn: static t => t.Reset());

    private static readonly TypedPool<BuffTurnTrigger> BuffTriggerPool = new(
        () => new BuffTurnTrigger(), 16, afterReturn: static t => t.Reset());

    private static readonly TypedPool<SkillTurnTrigger> SkillTriggerPool = new(
        () => new SkillTurnTrigger(), 16, afterReturn: static t => t.Reset());

    /// <summary>外部使用完触发者后主动归还到池。不归还也不会泄漏:池空时下次 Get 有害新建,借出对象由外部释放。</summary>
    public static void ReturnTrigger(TurnTrigger trigger)
    {
        switch (trigger)
        {
            case RegionTurnTrigger r: RegionTriggerPool.Return(r); break;
            case BuffTurnTrigger b: BuffTriggerPool.Return(b); break;
            case SkillTurnTrigger s: SkillTriggerPool.Return(s); break;
        }
    }
    public static void ReturnTriggers(IEnumerable<TurnTrigger> triggers)
    {
        foreach (var trigger in triggers)
        {
            ReturnTrigger(trigger);
        }
    }
    
    extension(IReadOnlyGameState state)
    {
        /// <summary>收集"下回合区域触发者"。</summary>
        public List<RegionTurnTrigger> CollectRegionTurnTriggers(
            bool isFocusedRegionTurn = false, Vector2I? focusCoords = null)
        {
            var turn = state.TurnState.CurrentTurn + 1;
            var list = new List<RegionTurnTrigger>();
            //var regionOrder = state.TurnState.GetPhaseOrder(turn, "RegionPhase");

            for (int x = 0; x < state.TileMapState.Size.X; x++)
            {
                for (int y = 0; y < state.TileMapState.Size.Y; y++)
                {
                    var coords = new Vector2I(x, y);
                    foreach (var region in state.TileMapState.GetTile(coords).Regions)
                    {
                        var info = region.RegionName.GetRegionInfo();
                        var layer = isFocusedRegionTurn || (focusCoords.HasValue && focusCoords.Value == coords)
                            ? SimulationLayer.Focused
                            : SimulationLayer.Waiting;
                        list.Add((RegionTurnTrigger)RegionTriggerPool.Get().Init(layer, 0, info.Priority,
                            coords, region.RegionName, region.RegionData, turn, info.Hook));
                    }
                }
            }

            return list;
        }

        /// <summary>收集"下回合单位Buff触发者"。进入层由 <paramref name="isFocusedBuffTurn"/> 决定(聚焦时为 Focused)。</summary>
        public List<BuffTurnTrigger> CollectBuffTurnTriggers(bool isFocusedBuffTurn = false)
        {
            var turn = state.TurnState.CurrentTurn + 1;
            var list = new List<BuffTurnTrigger>();
            if (!isFocusedBuffTurn) return list;

            //var buffOrder = state.TurnState.GetPhaseOrder(turn, "BuffPhase");
            foreach (var unit in state.UnitMapState.GetAllUnits())
            {
                foreach (var buff in unit.Buffs)
                {
                    var info = buff.BuffName.GetBuffInfo();
                    list.Add(BuffTriggerPool.Get().Init(SimulationLayer.Focused, 0,
                        info.Priority,
                        unit.Id, buff.BuffName, buff.Count, turn, info.Hook));
                }
            }

            return list;
        }

        /// <summary>
        /// 收集"下回合单位技能触发者"。
        /// <para>team 非空时仅收集该队伍单位的准备技能。</para>
        /// </summary>
        public List<SkillTurnTrigger> CollectSkillTurnTriggers(bool isFocusedSkillTurn = false, long? focusUnitId = null, int? team = null)
        {
            var turn = state.TurnState.CurrentTurn + 1;
            var list = new List<SkillTurnTrigger>();

            // 分组到 NaturePhase (未注册时安全回退到末位)
            /*var skillOrder = state.TurnState.PhaseOrders.TryGetValue(turn, out var skillPhases) &&
                             skillPhases.TryGetValue("NaturePhase", out var so)
                ? so
                : int.MaxValue;*/

            foreach (var unit in state.UnitMapState.GetAllUnits())
            {
                if (team.HasValue && unit.Team != team.Value) continue;
                if (unit.PrepareSkill is not { } skill) continue;
                var layer = isFocusedSkillTurn || (focusUnitId.HasValue && focusUnitId.Value == unit.Id)
                    ? SimulationLayer.Focused
                    : SimulationLayer.Waiting;
                list.Add(SkillTriggerPool.Get()
                    .Init(layer, 0, 0, unit.Id, unit.Team, skill.SkillName, skill.Args));
            }

            return list;
        }

        /// <summary>
        /// 收集"下回合触发者"（区域 + 单位Buff + 单位技能）的合并结果,供 DoUpdate 一次性执行。
        /// <para>返回列表中可能包含失效触发者,调用方应执行前用 <see cref="TurnTrigger.CheckIsValid"/> 过滤。</para>
        /// <para>实例来自对象池;调用方使用完需主动 <see cref="ReturnTrigger"/> 归还。不归还也不会泄漏。</para>
        /// </summary>
        public List<TurnTrigger> CollectNextTurnTriggers(
            Vector2I? focusCoords = null, long? focusUnitId = null,
            bool isFocusedRegionTurn = false,
            bool isFocusedBuffTurn = false,
            bool isFocusedSkillTurn = false)
        {
            var list = new List<TurnTrigger>(32);
            list.AddRange(CollectRegionTurnTriggers(state, isFocusedRegionTurn, focusCoords));
            list.AddRange(CollectBuffTurnTriggers(state, isFocusedBuffTurn));
            list.AddRange(CollectSkillTurnTriggers(state, isFocusedSkillTurn, focusUnitId));
            return list;
        }
    }
}