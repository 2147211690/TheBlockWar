using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool, Icon ("res://addons/at-icons/node/box.svg")]
public partial class TileInfo : Resource, IInfo
{
    public static readonly TileInfo Null = new();
    [Export] public string Name { get; set; } = "None";
    [Export] public Texture2D? Icon { get; set; }
    [Export] public RegionInitInfo[] InitRegions { get; set; } = [];
    [ExportGroup("View")]
    [Export] public float ViewHeight { get; set; } = 34;
    [Export] public Vector2 ViewOffset { get; set; } = new (0, -16);
    [Export] public SpriteFrames? ViewFrames { get; set; }
    [Export] public Color DominantColor { get; set; } = Colors.White;
    [Export] public float WaterHeight { get; set; } = 0;
}