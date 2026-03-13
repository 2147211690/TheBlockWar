using System.Collections.Generic;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class TilesView : TileMapLayer
{
    private readonly Dictionary<Vector2I, TileView> _tileViews = new();
    [Export] private Node2D _nodeContainer;
    [Export] private PackedScene _tileScene;
    [Export] public float NormalGroundHeight = 32f;
    public Vector2 LocalLeft { get; private set; }
    public Vector2 LocalUp { get; private set; }
    public Vector2 LocalRightDown{ get; private set; }

    public override void _Ready()
    {
        LocalLeft = MapToLocal(Vector2I.Left) - MapToLocal(Vector2I.Zero);
        LocalUp = MapToLocal(Vector2I.Up) - MapToLocal(Vector2I.Zero);
        LocalRightDown = MapToLocal(Vector2I.One) - MapToLocal(Vector2I.Zero);
    }

    public float GetTileHeight(Vector2I coords, Layer layer = Layer.Ground) =>
        (_tileViews.GetValueOrDefault(coords, null)?.Height ?? 0) + LocalRightDown.Y / (int)layer;
    public Vector2I GlobalMouseToTileCoords(Vector2 globalMousePos)
    {
        var local = ToLocal(globalMousePos);
        local.Y += NormalGroundHeight;
        return LocalToMap(local);
    }
    public GDTask PlaceTile(Vector2I coords, TileState tile)
    {
        //GD.Print("PlaceTile");
        if (!_tileViews.TryGetValue(coords, out var render))
        {
            render = _tileViews[coords] = _tileScene.Instantiate<TileView>();
            _nodeContainer.AddChild(render);
        }
        render.Position = MapToLocal(coords);
        if ((tile.Tag & TileTag.Empty) != 0)
        {
            render.TileName = "Empty";
        }
        else
        {
            render.TileName = "Soil";
            return render.PlayPlaceDownAnim();
        }
        return GDTask.CompletedTask;
    }
    public GDTask DropTile(Vector2I coords)
    {
        GD.Print("DropTile");
        TileView render;
        if (!_tileViews.TryGetValue(coords, out render))
        {
            return GDTask.CompletedTask;
        }
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Bounce); // 使用Bounce效果模拟弹性落地
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(render, "position", MapToLocal(coords) + new Vector2(0, -30), 0.08f);
        return tween.ToSignal(tween, "finished").AsGDTask();
    }

    public Vector2 MapToLocal(Vector2I coords, Layer layer) => MapToLocal(coords) - GetLayerOffset(layer);
    public Vector2 GetLayerOffset(Layer layer) => new(0, -LocalRightDown.Y / (int)layer);
}

public enum Layer
{
    Ground = 1,
    Underground = 4,
    Sky = 2,
}