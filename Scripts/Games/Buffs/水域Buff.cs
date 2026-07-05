using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 水域Buff() : Buff(BuffNames.水域)
{
    public override void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
        if (unit.FindBuff(BuffNames.燃烧) != -1)
        {
            obj.ApplyModifyUnitBuff(unit.Coords, BuffNames.燃烧, BuffOperation.RemoveBuff, -1);
        }
    }

    public override void OnUnitShouldUseSkill(IReadOnlyGameState state, IReadOnlyUnitState unit, int buffCount, ISkill skill,
        ref bool shouldUseSkill)
    {
        if (skill.SkillName.GetSkillInfo().Type == SkillTypeEnum.Move) return;
        shouldUseSkill = false;
    }
}