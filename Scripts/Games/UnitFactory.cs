using Godot;

namespace TheBlockWar.Scripts.Games;

public static class UnitFactory
{
    private static uint _nextUnitId = 0;

    public static uint CreateUnitId()
    {
        return _nextUnitId++;
    }

    public static UnitState CreateUnit(string name, Vector2I coords = default)
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
        };
    }
}