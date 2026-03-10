using System.Collections.Generic;
using Godot;
using GodotTask;

namespace TheBlockWar.Scripts.Views;

public partial class TileView : Node2D
{
    [Export] public AnimatedSprite2D MainSprite { get; set; }
    [Export] private Godot.Collections.Dictionary<string, float> _tilesHeights = new();
    [Export,ExportGroup("PlaceDown")] public float DropHeight { get; set; } = 30f;
    [Export,ExportGroup("PlaceDown")] public float DropDuration { get; set; } = 0.03f;
    public string TileName
    {
        get => MainSprite.Animation;
        set
        {
            MainSprite.Animation = value;
            MainSprite.Play(value);
            MainSprite.Offset = new Vector2(0, -Height / 2);
        }
    }
    public float Height => _tilesHeights.GetValueOrDefault(TileName, 0);
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