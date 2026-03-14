using System;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public class UnitState : IReadOnlyUnitState
{
    public static readonly UnitState Default = new();
    public uint Id { get; set; }
    public string Name { get; set; } =  string.Empty;
    public Vector2I Coords { get; set; }
    public float Health { get; set; } = 100;
    public float MaxHealth { get; set; } = 100;
    public float Shield { get; set; } = 100;
    public float MaxShield { get; set; } = 100;
    public bool IsDead { get; set; }


    public UnitState Clone()
    {
        return CopyTo(new UnitState());
    }
    public UnitState CopyTo(UnitState unit)
    {
        unit.Id = Id;
        unit.Name = Name;
        unit.Coords = Coords;
        unit.Health = Health;
        unit.MaxHealth = MaxHealth;
        unit.Shield = Shield;
        unit.MaxShield = MaxShield;
        unit.IsDead = IsDead;
        return unit;
    }
}