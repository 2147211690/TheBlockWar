using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 浸润Buff() : Buff(BuffNames.浸润)
{
    public override void OnUnitBeforeSkill(IGameState state, IReadOnlyUnitState unit, string skillName, ref bool canUseSkill,
        SimulationContext context)
    {
        canUseSkill = false;
    }
}