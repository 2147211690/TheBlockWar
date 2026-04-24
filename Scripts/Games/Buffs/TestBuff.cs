using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Buffs;

public class TestBuff() : Buff("res://Resources/Infos/BuffInfos/TestBuff.tres")
{
    public override void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage,
        SimulationContext context)
    {
        if (rawDamage == 0) return;
        resultDamage = 0;
        state.ApplyModifyUnitBuff(unit.Coords, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 0, context);
    }

    public override void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
        //state.UnitMapState.ModifyUnit(unit.Id, u => u);
    }
}