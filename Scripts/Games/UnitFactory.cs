using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;
//TODO:可能使用其他方式创建单位
public static class UnitFactory
{
    private static uint _nextUnitId = 1;

    public static uint CreateUnitId()
    {
        return _nextUnitId++;
    }

    public static UnitState CreateUnit(string name, int team ,Vector2I coords = default)
    {
        return new UnitState()
        {
            Id = CreateUnitId(),
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