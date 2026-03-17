
using System.Collections.Generic;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

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
        public virtual void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result)
        {
        }

        public virtual void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage, ref float resultDamage,
            SimulationResult result)
        {
        }

        public virtual void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage, SimulationResult result)
        {
        }

        public virtual void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure, ref float resultCure,
            SimulationResult result)
        {
        }

        public virtual void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure, SimulationResult result)
        {
        }

        public virtual void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result)
        {
        }

        public virtual void OnDurationChanged(IGameState state, IReadOnlyUnitState unit, int oldDuration, int newDuration,
            SimulationResult result)
        {
        }
    }
    
    class Test() : Buff(BuffTypeEnum.Test)
    {
        public override void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage, ref float resultDamage, 
            SimulationResult result)
        {
            resultDamage = 0;
            state.ApplyModifyUnitBuff(unit.Coords, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 0, result);
        }

        public override void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationResult result)
        {
            //state.UnitMapState.ModifyUnit(unit.Id, u => u);
        }
    }
}