using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 水域Buff() : Buff(BuffNames.水域)
{
    public override void OnUnitShouldUseSkill(IReadOnlyUnitState unit, ISkill skill, ref bool shouldUseSkill)
    {
        if (skill.SkillName.EndsWith("移动")) return;
        shouldUseSkill = false;
    }
}