using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Games.Turns.Phases;

namespace TheBlockWar.Scripts.Games.Simulations;

/// <summary>
/// 触发者比较器。排序规则(依据 <see cref="FromPhase"/> 从阶段集合构造):
/// 1. 先按 <see cref="TurnTrigger.Layer"/>;
/// 2. 再按触发者所属 <see cref="PhaseEnum"/> 在阶段迭代器中的顺序;
/// 3. 若是技能,再按 Player 阶段的 Team 顺序;
/// 4. 最后按 <see cref="TurnTrigger.Priority"/>。
/// </summary>
public static class TriggerComparer
{
    public static IComparer<TurnTrigger> FromPhase(IEnumerable<ITurnPhase> phases) => new FromPhaseComparer(phases);
    public static IComparer<TurnTrigger> Default => Comparer<TurnTrigger>.Create((a, b) =>
    {
        // 1. 先按 Layer
        var c = a.Layer.CompareTo(b.Layer);
        if (c != 0) return c;
        return a.Priority.CompareTo(b.Priority);
    });

    class FromPhaseComparer : IComparer<TurnTrigger>
    {
        /// <summary>阶段类型 → 在阶段迭代器中的顺序索引。</summary>
        private readonly IReadOnlyDictionary<PhaseEnum, int> _phaseTypeIndex;

        /// <summary>队伍 → 顺序索引(来自 Player 阶段的枚举顺序,供技能比较)。</summary>
        private readonly IReadOnlyDictionary<int, int> _teamOrder;

        /// <summary>
        /// 从阶段集合构造比较器:提取阶段类型的顺序(供 Phase 比较)与 Player 阶段的队伍顺序(供技能比较)。
        /// </summary>
        public FromPhaseComparer(IEnumerable<ITurnPhase> phases)
        {
            var phaseTypeIndex = new Dictionary<PhaseEnum, int>();
            var teamOrder = new Dictionary<int, int>();
            var phaseOrder = 0;
            var teamOrder2 = 0;
            foreach (var phase in phases)
            {
                if (!phaseTypeIndex.ContainsKey(phase.PhaseType))
                {
                    phaseTypeIndex[phase.PhaseType] = phaseOrder++;
                }

                // Player 阶段(如技能/队伍阶段)按队伍顺序登记,供 Skill 触发者比较
                if (phase.PhaseType == PhaseEnum.Skill && !teamOrder.ContainsKey(phase.Team))
                {
                    teamOrder[phase.Team] = teamOrder2++;
                }
            }
            _phaseTypeIndex = phaseTypeIndex;
            _teamOrder = teamOrder;
        }

        public int Compare(TurnTrigger? x, TurnTrigger? y)
        {
            if (ReferenceEquals(x, y)) return 0;
            if (x is null) return -1;
            if (y is null) return 1;

            // 1. 先按 Layer
            var c = x.Layer.CompareTo(y.Layer);
            if (c != 0) return c;

            // 2. 再按触发者所属 PhaseType 在阶段迭代器中的顺序
            c = PhaseIndex(x).CompareTo(PhaseIndex(y));
            if (c != 0) return c;

            // 3. 技能按 Player 阶段的 Team 顺序
            if (x is SkillTurnTrigger sx && y is SkillTurnTrigger sy)
            {
                c = TeamIndex(sx.UnitTeam).CompareTo(TeamIndex(sy.UnitTeam));
                if (c != 0) return c;
            }

            // 4. 最后按优先级(数值小优先)
            return x.Priority.CompareTo(y.Priority);
        }

        /// <summary>触发者所属的阶段类型——从实现类型映射到 <see cref="PhaseEnum"/>。</summary>
        private static PhaseEnum? PhaseTypeOf(TurnTrigger trigger) => trigger switch
        {
            BuffTurnTrigger => PhaseEnum.Buff,
            RegionTurnTrigger => PhaseEnum.Region,
            SkillTurnTrigger => PhaseEnum.Skill,
            _ => null,
        };

        private int PhaseIndex(TurnTrigger trigger)
        {
            var pt = PhaseTypeOf(trigger);
            return pt is { } value && _phaseTypeIndex.TryGetValue(value, out var index) ? index : _phaseTypeIndex.Count;
        }

        private int TeamIndex(int team)
            => _teamOrder.TryGetValue(team, out var index) ? index : _teamOrder.Count;
    }
}