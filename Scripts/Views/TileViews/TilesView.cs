using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.TileViews;
[GlobalClass]
public partial class TilesView : TileMapLayer
{
    private readonly Dictionary<Vector2I, TileView> _tileViews = new();

    /// <summary>
    /// 描述抽象高度到实际高度的映射关系
    /// </summary>
    public Vector2 LocalLeft { get; private set; }
    public Vector2 LocalUp { get; private set; }
    public Vector2 LocalRightDown{ get; private set; }
    public IGameView GameView { get; set; } = null!;
    private Dictionary<Vector2I, (RegionShowTypeEnum, int)> _regionViews = new();
    private List<ViewSprite2D> _regionSprites = new();
    public override void _Ready()
    {
        LocalLeft = MapToLocal(Vector2I.Left) - MapToLocal(Vector2I.Zero);
        LocalUp = MapToLocal(Vector2I.Up) - MapToLocal(Vector2I.Zero);
        LocalRightDown = MapToLocal(Vector2I.One) - MapToLocal(Vector2I.Zero);
    }
    public void ClearAllTile()
    {
        foreach (var render in _tileViews.Values)
        {
            GameView.Pools.TileViewPool.Return(render);
        }
        _tileViews.Clear();
    }
    public float GetTileHeight(in Vector2I coords) => _tileViews!.GetValueOrDefault(coords, null)?.Height ?? 0;
    public TileView? GetTileView(Vector2I coords) => _tileViews.TryGetValue(coords, out var render) ?
        (render is not {TileName: "Air"} ? render : null) :
        null;
    
    public TileView? SetTileView(Vector2I coords, string tileName)
    {
        if (tileName == "Air")
        {
            var tile = _tileViews.GetValueOrDefault(coords);
            if (tile is not null)
            {
                _tileViews.Remove(coords);
                GameView.Pools.TileViewPool.Return(tile);
            }
            return null;
        }
        if (!_tileViews.TryGetValue(coords, out var render))
        {
            render = _tileViews[coords] = GameView.Pools.TileViewPool.Get();
        }
        render.GamePosition = CoordsToGamePosition(coords, 0, 0);
        render.Visible = true;
        render.TileName = tileName;
        GameView.GetUnitView(coords)?.ResetToWishCoords();
        return render;
    }

    public Vector2 GamePositionToNodePosition(in Vector3 gamePosition)
    {
        var lx = Mathf.FloorToInt(gamePosition.X);
        var ly = Mathf.FloorToInt(gamePosition.Y);
        var p00 = MapToLocal(new Vector2I(lx, ly));
        var tx = gamePosition.X - lx;
        var ty = gamePosition.Y - ly;

        return (p00 + -LocalLeft * tx + -LocalUp * ty) + new Vector2(0, -gamePosition.Z);
    }
    
    public Vector3 CoordsToGamePosition(Vector2 coords, float heightOffset = 0, float? height = null)
    {
        var z = height ?? GetTileHeight(new(Mathf.RoundToInt(coords.X), Mathf.RoundToInt(coords.Y)));
        return coords.ToVec3(z + heightOffset);
    }

    public Vector2I NodePositionToCoords(Vector2 nodePosition)
    {
        var local = ToLocal(nodePosition);
        local.Y += IReadOnlyTileState.MaxTileHeight;
        return LocalToMap(local);
    }
    
    public TileInfo GetTileInfo(in Vector2I coords) 
        => _tileViews!.GetValueOrDefault(coords, null)?.TileName.GetTileInfo() ?? TileInfo.Null;
    
    public void ShowRegion(IEnumerable<Vector2I> coords, int team, RegionShowTypeEnum showTypeEnum)
    {
        foreach (var coord in coords)
        {
            _regionViews[coord] = (showTypeEnum, team);
        }
        UpdateRegionView();
    }
    public void ClearRegion()
    {
        _regionViews.Clear();
        UpdateRegionView();
    }
    private void UpdateRegionView()
    {
        var spIndex = 0;
        foreach (var (coords, (showTypeEnum, team)) in _regionViews)
        {
            var teamStyle = GameView.GameStyle[GameView.GetTeamType(team)];
            var sprite = GetSprite();
            sprite.GamePosition = GameView.CoordsToGamePosition(coords);
            sprite.Modulate = teamStyle.SkillRegionColor;
            sprite.Texture = teamStyle.SkillRegionTexture;
            sprite.GameZIndex = -3;
        }
        for (int i = _regionSprites.Count - 1; i >= spIndex; i--)
        {
            GameView.Pools.Sprite2DPool.Return(_regionSprites[i]);
            _regionSprites.RemoveAt(i);
        }

        ViewSprite2D GetSprite()
        {
            if (spIndex < _regionSprites.Count)
            {
                return _regionSprites[spIndex++];
            }
            else
            {
                var s = GameView.Pools.Sprite2DPool.Get();
                _regionSprites.Add(s);
                spIndex += 1;
                return s;
            }
        }
    }
}