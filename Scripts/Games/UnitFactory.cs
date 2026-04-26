using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Units;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games;
//TODO:可能使用其他方式创建单位
public static class UnitFactory
{
    public static UnitState CreateUnit(UnitNameEnum name, int team, long id, Vector2I coords = default)
    {
        return new UnitState()
        {
            Id = id,
            Name = name,
            Coords = coords,
            MaxHealth = 100,
            Health = 100,
            MaxShield = 100,
            Shield = 0,
            Team = team,
        };
    }
}