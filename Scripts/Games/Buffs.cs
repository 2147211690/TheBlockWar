
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
    
    
    class Test : IBuff
    {
        public BuffTypeEnum BuffType => BuffTypeEnum.Test;
        public void OnAddToUnit(IGameState state, uint id, SimulationResult result)
        {
        }

        public void OnUnitBeforeDamage(IGameState state, uint id, float rawDamage, ref float resultDamage, SimulationResult result)
        {
            var unit = state.UnitMapState.GetUnit(id);
            resultDamage = 0;
            state.ApplyModifyUnitBuff(unit.Coords, BuffTypeEnum.Test, BuffOperation.RemoveBuff, 0, result);
        }

        public void OnUnitAfterDamage(IGameState state, uint id, float getDamage, SimulationResult result)
        {
            state.ApplyDropTile(state.UnitMapState.GetUnit(id)!.Coords, result);
        }

        public void OnRemoveFromUnit(IGameState state, uint id, SimulationResult result)
        {
        }
    }
}