using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class 贤者的庇护Buff() : Buff(BuffNames.贤者的庇护)
{
    public override void OnAddToUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
        obj.ApplyModifyGlobalBuff(unit.Name, BuffNames.贤者的庇护, 1 << unit.Team, BuffNames.贤者的庇护光环 ,true);
    }
    public override void OnRemoveFromUnit(SimObject obj, IReadOnlyUnitState unit, int buffCount)
    {
        obj.ApplyModifyGlobalBuff(unit.Name, BuffNames.贤者的庇护, 1 << unit.Team, BuffNames.贤者的庇护光环 ,false);
    }
}