
using System.Collections.Generic;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public static class Buffs
{
    private static readonly Dictionary<BuffTypeEnum, IBuff> _buffs = new();
    public static void Register(IBuff buff)
    {
        _buffs[buff.BuffType] = buff;
    }
    public static IBuff GetInstance(this BuffTypeEnum type)
    {
        return _buffs[type];
    }

    static Buffs()
    {
        Register(new Test());
    }
    
    class Buff : IBuff
    {
        public Buff(BuffTypeEnum buffType)
        {
            BuffType = buffType;
        }

        public virtual BuffTypeEnum BuffType { get; }

        public virtual void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result,
            out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public virtual void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage, ref float resultDamage,
            SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public virtual void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, SimulationResult result,
            out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public virtual void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure, ref float resultCure,
            SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public virtual void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure, SimulationResult result,
            out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public virtual void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result,
            out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }

        public void OnDurationChanged(IGameState state, IReadOnlyUnitState unit, int oldDuration, int newDuration,
            SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            renewedUnit = unit;
        }
    }
    
    class Test() : Buff(BuffTypeEnum.Test)
    {
        public override void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage, ref float resultDamage, 
            SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            resultDamage = 0;
            state.ApplyModifyUnitBuff(unit.Coords, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 0, result);
            renewedUnit = state.UnitMapState.GetUnit(unit.Id);
        }

        public override void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            state.ApplyDropTile(unit.Coords, result);
            renewedUnit = state.UnitMapState.GetUnit(unit.Id);
        }
    }
}