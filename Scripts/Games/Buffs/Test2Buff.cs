using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Buffs;

public class Test2Buff() : Buff(BuffNames.Test2)
{
    public override void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage,
        SimulationContext context)
    {
        if (rawDamage == 0) return;
        resultDamage = 0;
        state.ApplyModifyUnitBuff(unit.Coords, Name, BuffOperation.RemoveBuff, 0, context);
    }

    public override void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
        //state.UnitMapState.ModifyUnit(unit.Id, u => u);
    }

    public override void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context)
    {
        base.OnEnterTurn(state, unit, turn, context);
        state.ApplyPushUnit(unit.Coords, DirMask.Down, context);
    }
}