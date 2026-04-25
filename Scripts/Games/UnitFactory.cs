using TheBlockWar.Scripts.Games.GameStates;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games;
//TODO:可能使用其他方式创建单位
public static class UnitFactory
{
    public static UnitState CreateUnit(string name, int team, long id, Vector2I coords = default)
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