using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Turns;
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

/// <summary>放置触发者：某队伍本回合计划在某个格子放置一个单位。</summary>
public sealed class PlaceUnitTurnTrigger : TurnTrigger
{
    /// <summary>放置单位所属队伍。</summary>
    public int Team { get; private set; }
    private int _index;
    public PlaceUnitTurnTrigger Init(SimulationLayer layer, int phaseOrder, int priority,
        int team, int index, Vector2I coords, string unitName)
    {
        base.Init(layer, phaseOrder, priority, $"放置:{unitName}@{coords}");
        Team = team;
        _index = index;
        return this;
    }

    public override void Reset()
    {
        base.Reset();
        _index = 0;
    }

    public override bool CheckIsValid(SimObject obj)
    {
        var place = obj.State.TeamState.GetTeamPreparePlaceUnit(Team, _index);
        return place is { };
    }

    public override void Invoke(SimObject obj)
    {
        obj.ApplyPlacePrepareUnit(Team, _index);
    }
}

public static class TurnTriggerCollector
{
    /// <summary>共享默认顺序表(未传入时使用;不会填充 PhaseOrder 时按 int.MaxValue)。</summary>
    private static readonly PhaseOrderTable DefaultPhaseOrderTable = new();

    /// <summary>简单排序:先 Layer,再 PhaseOrder,最后 Priority。</summary>
    private static readonly IComparer<TurnTrigger> OrderComparer = Comparer<TurnTrigger>.Create((a, b) =>
    {
        var c = a.Layer.CompareTo(b.Layer);
        if (c != 0) return c;
        c = a.PhaseOrder.CompareTo(b.PhaseOrder);
        if (c != 0) return c;
        return a.Priority.CompareTo(b.Priority);
    });
    /// <summary>各触发者的对象池;AfterReturn 时清空引用以安全复用。</summary>
    private static readonly TypedPool<RegionTurnTrigger> RegionTriggerPool = new(
        () => new RegionTurnTrigger(), 16, afterReturn: static t => t.Reset());

    private static readonly TypedPool<BuffTurnTrigger> BuffTriggerPool = new(
        () => new BuffTurnTrigger(), 16, afterReturn: static t => t.Reset());

    private static readonly TypedPool<SkillTurnTrigger> SkillTriggerPool = new(
        () => new SkillTurnTrigger(), 16, afterReturn: static t => t.Reset());

    private static readonly TypedPool<PlaceUnitTurnTrigger> PlaceTriggerPool = new(
        () => new PlaceUnitTurnTrigger(), 16, afterReturn: static t => t.Reset());

    /// <summary>外部使用完触发者后主动归还到池。不归还也不会泄漏:池空时下次 Get 有害新建,借出对象由外部释放。</summary>
    public static void ReturnTrigger(TurnTrigger trigger)
    {
        switch (trigger)
        {
            case RegionTurnTrigger r: RegionTriggerPool.Return(r); break;
            case BuffTurnTrigger b: BuffTriggerPool.Return(b); break;
            case SkillTurnTrigger s: SkillTriggerPool.Return(s); break;
            case PlaceUnitTurnTrigger p: PlaceTriggerPool.Return(p); break;
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
            PhaseOrderTable orderTable, bool isFocusedRegionTurn = false, Vector2I? focusCoords = null)
        {
            var table = orderTable ?? DefaultPhaseOrderTable;
            var turn = state.TurnState.CurrentTurn + 1;
            var order = table.GetRegionOrder();
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
                        list.Add((RegionTurnTrigger)RegionTriggerPool.Get().Init(layer, order, info.Priority,
                            coords, region.RegionName, region.RegionData, turn, info.Hook));
                    }
                }
            }

            list.Sort(OrderComparer);
            return list;
        }

        /// <summary>收集"下回合单位Buff触发者"。进入层由 <paramref name="isFocusedBuffTurn"/> 决定(聚焦时为 Focused)。</summary>
        public List<BuffTurnTrigger> CollectBuffTurnTriggers(PhaseOrderTable orderTable, bool isFocusedBuffTurn = false)
        {
            var table = orderTable ?? DefaultPhaseOrderTable;
            var turn = state.TurnState.CurrentTurn + 1;
            var order = table.GetBuffOrder();
            var list = new List<BuffTurnTrigger>();
            if (!isFocusedBuffTurn) return list;

            //var buffOrder = state.TurnState.GetPhaseOrder(turn, "BuffPhase");
            foreach (var unit in state.UnitMapState.GetAllUnits())
            {
                foreach (var buff in unit.Buffs)
                {
                    var info = buff.BuffName.GetBuffInfo();
                    list.Add(BuffTriggerPool.Get().Init(SimulationLayer.Focused, order,
                        info.Priority,
                        unit.Id, buff.BuffName, buff.Count, turn, info.Hook));
                }
            }

            list.Sort(OrderComparer);
            return list;
        }

        /// <summary>
        /// 收集"下回合单位技能触发者"。
        /// <para>聚焦判定:命中的队伍(掩码位)或指定单位 → Focused,其余 → Waiting。</para>
        /// <para>team 非空时仅收集该队伍的触发者。</para>
        /// </summary>
        public List<SkillTurnTrigger> CollectSkillTurnTriggers(PhaseOrderTable orderTable, int focusedSkillTeamMask = 0, long? focusUnitId = null, int? team = null)
        {
            var table = orderTable ?? DefaultPhaseOrderTable;
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
                var layer = ((focusedSkillTeamMask & (1 << unit.Team)) != 0) ||
                            (focusUnitId.HasValue && focusUnitId.Value == unit.Id)
                    ? SimulationLayer.Focused
                    : SimulationLayer.Waiting;
                var order = table.GetSkillOrder(unit.Team);
                list.Add(SkillTriggerPool.Get()
                    .Init(layer, order, 0, unit.Id, unit.Team, skill.SkillName, skill.Args));
            }

            list.Sort(OrderComparer);
            return list;
        }

        /// <summary>
        /// 收集"下回合队伍放置触发者"。
        /// <para>聚焦判定:命中的队伍(掩码位) → Focused,其余 → Waiting。</para>
        /// <para>team 非空时仅收集该队伍的放置。</para>
        /// </summary>
        public List<PlaceUnitTurnTrigger> CollectPlaceUnitTurnTriggers(PhaseOrderTable orderTable,Vector2I? focusCoords
            ,int focusedPlaceUnitTeamMask = 0, int? team = null)
        {
            var table = orderTable ?? DefaultPhaseOrderTable;
            var turn = state.TurnState.CurrentTurn + 1;
            var list = new List<PlaceUnitTurnTrigger>();
            // 阶段序号:接入 PlaceUnit 阶段(未注册时安全回退到末位,此处以 0 占位,排序由 TriggerComparer 决定)
            foreach (var t in state.TeamState.GetPrepareTeams())
            {
                if (team.HasValue && t != team.Value) continue;
                var layer = (focusedPlaceUnitTeamMask & (1 << t)) != 0
                    ? SimulationLayer.Focused
                    : SimulationLayer.Waiting;
                var places = state.TeamState.GetTeamPreparePlaceUnits(t);
                var order = table.GetPlaceUnitOrder(t);
                for (int i = 0; i < places.Count; i++)
                {
                    if (places[i] is not { } info) continue;
                    list.Add(PlaceTriggerPool.Get().Init(focusCoords is {} a && a == info.Coords ?
                            SimulationLayer.Focused : 
                            layer, order,
                        0, t, i, info.Coords, info.UnitName));
                }
            }

            list.Sort(OrderComparer);
            return list;
        }

        /// <summary>
        /// 收集"下回合触发者"（区域 + 单位Buff + 单位技能 + 队伍放置）的合并结果,按 (Layer, PhaseOrder, Priority) 排序后返回。
        /// <para>返回列表中可能包含失效触发者,调用方应执行前用 <see cref="TurnTrigger.CheckIsValid"/> 过滤。</para>
        /// <para>实例来自对象池;调用方使用完需主动 <see cref="ReturnTrigger"/> 归还。不归还也不会泄漏。</para>
        /// </summary>
        public List<TurnTrigger> CollectNextTurnTriggers(
            PhaseOrderTable orderTable,
            Vector2I? focusCoords = null, long? focusUnitId = null,
            bool isFocusedRegionTurn = false,
            bool isFocusedBuffTurn = false,
            int focusedSkillTeamMask = 0,
            int focusedPlaceUnitTeamMask = 0)
        {
            var list = new List<TurnTrigger>(32);
            list.AddRange(CollectRegionTurnTriggers(state, orderTable, isFocusedRegionTurn, focusCoords));
            list.AddRange(CollectBuffTurnTriggers(state, orderTable, isFocusedBuffTurn));
            list.AddRange(CollectSkillTurnTriggers(state, orderTable, focusedSkillTeamMask, focusUnitId));
            list.AddRange(CollectPlaceUnitTurnTriggers(state, orderTable, focusCoords, focusedPlaceUnitTeamMask));
            list.Sort(OrderComparer);
            return list;
        }
    }
}