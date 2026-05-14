using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class TileInfo : Resource, IInfo
{
    public static readonly TileInfo Null = new();
    [Export] public string Name { get; set; } = "None";
    [Export] public string[]? Regions { get; set; }
    [ExportGroup("View")]
    [Export] public float ViewHeight { get; set; } = 34;
    [Export] public Vector2 ViewOffset { get; set; } = new (0, -16);
    [Export] public SpriteFrames? ViewFrames { get; set; }
    
    #if TOOLS
    [ExportToolButton("使用高度一半偏移")]
    private Callable UseHalfHeightOffset => Callable.From(OnUseHalfHeightOffset);
    
    public void OnUseHalfHeightOffset()
    {
        ViewOffset = new (0, -ViewHeight / 2);
    }
    #endif
}