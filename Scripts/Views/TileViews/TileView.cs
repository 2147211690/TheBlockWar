using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.TileViews;
[Icon("res://addons/at-icons/node2d/box.svg")]
public partial class TileView : ViewNode2D
{
    [ExportGroup("Self")]
    [Export] public AnimatedSprite2D MainSprite { get; set; } = null!;
    public override Node2D RenderNode => MainSprite;
    public string TileName
    {
        get => _tileName;
        set
        {
            _tileName = value;
            
            _tileInfo = _tileName.GetTileInfo();
            MainSprite.SpriteFrames = _tileInfo.ViewFrames;
            MainSprite.Offset = _tileInfo.ViewOffset;
            MainSprite.TryPlay("idle");
        }
    }
    private string _tileName = "Air";
    private TileInfo _tileInfo = TileInfo.Null;
    public float Height => _tileInfo.ViewHeight;
}