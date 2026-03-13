using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyUnitState
{
    uint Id { get; }
    string Name { get; }
    Vector2I Coords { get; }
    float Health { get; }
    float MaxHealth { get; }
    float Shield { get; }
    float MaxShield { get; }
    bool IsDead { get; }
    UnitState Clone();
}