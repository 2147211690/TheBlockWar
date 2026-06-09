using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

public partial class TileView : ViewNode2D
{
    [Export] public AnimatedSprite2D MainSprite { get; set; } = null!;
    [ExportGroup("PlaceDown")]
    [Export] public float DropHeight { get; set; } = 30f;
    [Export] public float DropDuration { get; set; } = 0.03f;
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
    private string _tileName = string.Empty;
    private TileInfo _tileInfo = TileInfo.Null;
    
    public float Height => _tileInfo.ViewHeight;
    public GDTask PlayPlaceDownAnim()
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce);
        tween.SetEase(Tween.EaseType.Out);
        // 初始位置：目标上方
        MainSprite.Position = new Vector2(0, -DropHeight);
        // 动画：坠落到底部
        tween.TweenProperty(MainSprite, "position", Vector2.Zero, DropDuration);
        return ToSignal(tween, Tween.SignalName.Finished).AsGDTask();
    }
}