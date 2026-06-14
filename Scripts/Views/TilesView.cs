using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class TilesView : TileMapLayer
{
    private readonly Dictionary<Vector2I, TileView> _tileViews = new();
    [Export] public GameView GameView { get; set; } = null!;
    [Export] private GameViewNodeContainer _nodeContainer = null!;
    [Export] private PackedScene _tileScene = null!;
    /// <summary>
    /// 描述抽象高度到实际高度的映射关系
    /// </summary>
    [Export] public float NormalGroundHeight = 32f;
    [Export] public Texture2D? RegionTexture { get; set; }
    public Vector2 LocalLeft { get; private set; }
    public Vector2 LocalUp { get; private set; }
    public Vector2 LocalRightDown{ get; private set; }
    
    private RecordTypedPool<ViewSprite2D> _regionSpritePool = null!;
    public override void _Ready()
    {
        LocalLeft = MapToLocal(Vector2I.Left) - MapToLocal(Vector2I.Zero);
        LocalUp = MapToLocal(Vector2I.Up) - MapToLocal(Vector2I.Zero);
        LocalRightDown = MapToLocal(Vector2I.One) - MapToLocal(Vector2I.Zero);
        _regionSpritePool = new RecordTypedPool<ViewSprite2D>(() =>
        {
            var s = new ViewSprite2D();
            _nodeContainer.AddChild(s);
            s.Texture = RegionTexture;
            return s;
        }, 0)
        {
            AfterReturn = s =>
            {
                s.Visible = false;
            },
            BeforeGet = s =>
            {
                s.Visible = true;
            }
        };
    }

    public void ClearAllTile()
    {
        foreach (var render in _tileViews.Values)
        {
            render.QueueFree();
        }
        _tileViews.Clear();
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
    public float GetTileHeight(in Vector2I coords) => _tileViews!.GetValueOrDefault(coords, null)?.Height ?? 0;
    public Vector2I GlobalMouseToTileCoords(Vector2 globalMousePos)
    {
        var local = ToLocal(globalMousePos);
        local.Y += NormalGroundHeight;
        return LocalToMap(local);
    }
    public async GDTask PlaceTile(Vector2I coords, string tileName)
    {
        //GD.Print("PlaceTile");
        if (!_tileViews.TryGetValue(coords, out var render))
        {
            render = _tileViews[coords] = _tileScene.Instantiate<TileView>();
            _nodeContainer.AddChild(render);
        }
        render.GamePosition = GetGamePosition(coords);
        render.Visible = true;
        render.TileName = tileName;
        if (tileName != TileNames.Air) await render.PlayPlaceDownAnim();
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
            render.TileName = "Air";
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
                var s = _regionSpritePool.Get();
                s.GamePosition = GetGamePositionOnGround(coord);
                s.GameZIndex = -2;
                s.Modulate = new(0, 1, 0, 0.2f);
            }
        }
    }

    private float GetLayerOffset(Layer layer) => layer switch
    {
        Layer.Ground => 0,
        Layer.Underground => LocalRightDown.Y / 4,
        Layer.Sky => LocalRightDown.Y * 2,
        _ => 0,
    };

    public Vector2 GamePlanePositionToNodePosition(in Vector2 coords)
    {
        var lx = Mathf.FloorToInt(coords.X);
        var ly = Mathf.FloorToInt(coords.Y);
        var p00 = MapToLocal(new Vector2I(lx, ly));
        var tx = coords.X - lx;
        var ty = coords.Y - ly;

        return p00 + -LocalLeft * tx + -LocalUp * ty;
    }
    public Vector3 GetGamePosition(in Vector2 coords, float height = 0)
    {
        return new(coords.X, coords.Y, height);
    }
    public Vector3 GetGamePositionOnGround(in Vector2 coords, float heightOffset = 0)
    {
        var cInt = new Vector2I(Mathf.RoundToInt(coords.X), Mathf.RoundToInt(coords.Y));
        return GetGamePosition(coords, GetTileHeight(cInt) + heightOffset);
    }

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
    public TileInfo GetTileInfo(in Vector2I coords) 
        => _tileViews!.GetValueOrDefault(coords, null)?.TileName.GetTileInfo() ?? TileInfo.Null;
}
[Flags]
public enum Layer
{
    Ground = 1,
    Underground = 4,
    Sky = 2,
}