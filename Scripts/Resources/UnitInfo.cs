using TheBlockWar.Scripts.Games.Units;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class UnitInfo : Resource, IInfo
{
    public static readonly UnitInfo Null = new();
    [Export] public string Name { get; set; } = "None";
    [ExportGroup("Unit")]
    [Export] public float MaxHealth { get; set; }
    [Export] public float MaxShield { get; set; }
    [Export] public bool IsFlying { get; set; }
    [ExportGroup("View")]
    [Export] public Vector2 ViewOffset { get; set; }
    [Export] public SpriteFrames? ViewFrames { get; set; }
    
}