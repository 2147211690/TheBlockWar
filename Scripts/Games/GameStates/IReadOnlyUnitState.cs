using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitState
{
    uint Id { get; }
    int Team { get; }
    string Name { get; }
    Vector2I Coords { get; }
    float Health { get; }
    float MaxHealth { get; }
    float Shield { get; }
    float MaxShield { get; }
    bool IsDead { get; }
    bool IsFlying { get; set; }
    UnitState Clone();
    UnitState CopyTo(UnitState unit);
}