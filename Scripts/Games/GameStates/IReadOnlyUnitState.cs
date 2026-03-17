using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitState
{
    public static readonly IReadOnlyUnitState Null = new UnitState()
    {
        Id = 0,
        Coords = new Vector2I(int.MinValue, int.MinValue),
        IsDead = true,
    };
    uint Id { get; }
    int Team { get; }
    string Name { get; }
    Vector2I Coords { get; }
    float Health { get; }
    float MaxHealth { get; }
    float Shield { get; }
    float MaxShield { get; }
    bool IsDead { get; }
    bool IsFlying { get; }
    IReadOnlyList<BuffState> Buffs { get; }
    UnitState Clone();
    UnitState CopyTo(UnitState unit);
}