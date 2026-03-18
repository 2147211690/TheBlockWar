using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Buffs;

public class Test2Buff() : Buff("res://Resources/Infos/BuffInfos/Test2Buff.tres")
{
    public override void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage,
        SimulationResult result)
    {
        resultDamage = 0;
        state.ApplyModifyUnitBuff(unit.Coords, Info.BuffType, BuffOperation.RemoveBuff, 0, result);
    }

    public override void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result)
    {
        //state.UnitMapState.ModifyUnit(unit.Id, u => u);
    }
}