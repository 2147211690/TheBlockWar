using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 水域Buff() : Buff(BuffNames.水域)
{
    public override void OnUnitBeforeSkill(IReadOnlyGameState state, IReadOnlyUnitState unit, string skillName,
        ref bool canUseSkill)
    {
        if (skillName == SkillNames.Test1) return;
        canUseSkill = false;
    }
}