using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class TilesView : TileMapLayer
{
    private readonly Dictionary<Vector2I, TileView> _tileViews = new();
    [Export] public GameView GameView { get; set; }
    [Export] private Node2D _nodeContainer;
    [Export] private PackedScene _tileScene;
    [Export] public float NormalGroundHeight = 32f;
    [Export] public Texture2D? RegionTexture { get; set; }
    public Vector2 LocalLeft { get; private set; }
    public Vector2 LocalUp { get; private set; }
    public Vector2 LocalRightDown{ get; private set; }
    
    private RecordTypedPool<Sprite2D> _regionSpritePool;
    public override void _Ready()
    {
        LocalLeft = MapToLocal(Vector2I.Left) - MapToLocal(Vector2I.Zero);
        LocalUp = MapToLocal(Vector2I.Up) - MapToLocal(Vector2I.Zero);
        LocalRightDown = MapToLocal(Vector2I.One) - MapToLocal(Vector2I.Zero);
        _regionSpritePool = new RecordTypedPool<Sprite2D>(() =>
        {
            var s = new Sprite2D();
            _nodeContainer.AddChild(s);
            s.Texture = RegionTexture;
            return s;
        }, 0)
        {
            AfterPush = s =>
            {
                s.Visible = false;
            },
            BeforePop = s =>
            {
                s.Visible = true;
            }
        };
    }

    public Vector2 GetLocalVec(DirMask dirMask)
    {
        return dirMask switch
        {
            DirMask.Up => LocalUp,
            DirMask.Down => -LocalUp,
            DirMask.Left => LocalLeft,
            DirMask.Right => -LocalLeft,
            _ => Vector2.Zero,
        };
    }
    public float GetTileHeight(in Vector2I coords) => _tileViews.GetValueOrDefault(coords, null)?.Height ?? 0;
    public Vector2I GlobalMouseToTileCoords(Vector2 globalMousePos)
    {
        var local = ToLocal(globalMousePos);
        local.Y += NormalGroundHeight;
        return LocalToMap(local);
    }
    public async GDTask PlaceTile(Vector2I coords, TileState tile)
    {
        //GD.Print("PlaceTile");
        if (!_tileViews.TryGetValue(coords, out var render))
        {
            render = _tileViews[coords] = _tileScene.Instantiate<TileView>();
            _nodeContainer.AddChild(render);
        }
        render.Position = MapToLocal(coords);
        render.Visible = true;
        if ((tile.Tag & TileTag.Empty) != 0)
        {
            render.TileName = "Empty";
        }
        else
        {
            render.TileName = "Soil";
            await render.PlayPlaceDownAnim();
        }
        GameView.UnitsView.UpdateUnitPosition(coords);
    }
    public async GDTask DropTile(Vector2I coords)
    {
        GD.Print("DropTile");
        if (!_tileViews.TryGetValue(coords, out var render))
        {
            return;
        }
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce); // 使用Bounce效果模拟弹性落地
        tween.SetEase(Tween.EaseType.Out);
        var targetOffset = render.MainSprite.Offset + new Vector2(0, 30);
        tween.TweenProperty(render.MainSprite, "offset", targetOffset, 0.08f);
        tween.TweenProperty(render, "visible", false, 0.08f);
        tween.Finished += () =>
        {
            render.Visible = false;
            render.TileName = "Empty";
        };
        await tween.WaitForTween();
        GameView.UnitsView.UpdateUnitPosition(coords);
    }
    public void DrawRegions(IEnumerable<(HashSet<Vector2I> Coords, int type)> regions, in Vector2I mapSize)
    {
        _regionSpritePool.PushAllPopedItems();
        foreach (var (coords, type) in regions)
        {
            foreach (var coord in coords)
            {
                if (!coord.IsInSize(mapSize)) continue;
                var s = _regionSpritePool.Pop();
                s.Position = GetRenderPosition(coord, Layer.Underground, -2);
                s.Offset = GetRenderOffset(coord, Layer.Underground, -2);
                s.Modulate = new(0, 1, 0, 0.2f);
            }
        }
    }

    private float GetLayerOffset(Layer layer) => layer switch
    {
        Layer.Ground => 0,
        Layer.Underground => LocalRightDown.Y / 4,
        Layer.Sky => LocalRightDown.Y * 2,
    };
    
    public Vector2 GetRealPosition(in Vector2I coords, Layer layer, float? defineHeight = null)
    {
        var pos = MapToLocal(coords);
        if (defineHeight == null)
        {
            if ((layer & Layer.Underground) != 0 || (layer & Layer.Sky) != 0) pos.Y -= GetTileHeight(coords);
        }
        else
        {
            pos.Y -= defineHeight.Value;
        }
        return pos;
    }
    
    public Vector2 GetRenderPosition(in Vector2I coords, Layer layer, int renderOffset)
    {
        var maxOffset = GetLayerOffset(layer);
        var pos = MapToLocal(coords);
        pos.Y += maxOffset + renderOffset / 100f * maxOffset;
        return pos;
    }

    public Vector2 GetRenderOffset(in Vector2I coords, Layer layer, int renderOffset, float? defineHeight = null)
    {
        var maxOffset = GetLayerOffset(layer);
        var pos = new Vector2(0, -maxOffset - renderOffset / 100f * maxOffset);
        if (defineHeight == null)
        {
            if ((layer & Layer.Underground) != 0 || (layer & Layer.Sky) != 0) pos.Y -= GetTileHeight(coords);
        }
        else
        {
            pos.Y -= defineHeight.Value;
        }
        return pos;
    }
}
[Flags]
public enum Layer
{
    Ground = 1,
    Underground = 4,
    Sky = 2,
}