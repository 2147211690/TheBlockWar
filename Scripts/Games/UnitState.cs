using Godot;

namespace TheBlockWar.Scripts.Games;

public struct UnitState
{
    public static readonly UnitState Default = new();
    public uint Id { get; set; }
    public string Name { get; set; }
    public Vector2I Coords { get; set; }
    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float Shield { get; set; }
    public float MaxShield { get; set; }
    public bool IsDead { get; set; }


    public UnitState Clone()
    {
        return new UnitState
        {
            Id = Id,
            Coords = Coords,
        };
    }

}