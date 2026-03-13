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
        return new UnitState
        {
            Id = Id,
            Name = Name,
            Coords = Coords,
            Health = Health,
            MaxHealth = MaxHealth,
            Shield = Shield,
            MaxShield = MaxShield,
            IsDead = IsDead,
        };
    }

}