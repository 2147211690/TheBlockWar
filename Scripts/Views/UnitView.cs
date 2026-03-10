using System.Collections.Generic;
using Godot;
using GodotTask;

namespace TheBlockWar.Scripts.Views;

public partial class UnitView : Node2D
{
    [Export] public Godot.Collections.Dictionary<string, Vector2> Offsets = new();
    [Export] public AnimatedSprite2D MainSprite { get; set; }
    [Export] public Node2D Center { get; set; }
    
    [Export,ExportGroup("PlaceDown")] public float DropHeight { get; set; } = 50f;
    [Export,ExportGroup("PlaceDown")] public float DropDuration { get; set; } = 0.1f;
    [Export,ExportGroup("PlaceDown")] public float ImpactSquashY { get; set; } = 1.3f;
    [Export,ExportGroup("PlaceDown")] public float ImpactStretchX { get; set; } = 0.25f;
    [Export,ExportGroup("PlaceDown")] public float RecoveryDuration { get; set; } = 0.04f;
    public string UnitName
    {
        get;
        set
        {
            MainSprite.Animation = value;
            MainSprite.Offset = Offsets.GetValueOrDefault(value, Vector2.Zero);
            field = value;
        }
    }

    public void SetGroundHeight(float height)
    {
        Center.Position = new Vector2(0, -height);
    }

    public GDTask PlayPlaceDownAnim()
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.In);
        
        // 起始位置：上方
        MainSprite.Position = new Vector2(0, -DropHeight);
        MainSprite.Scale = Vector2.One;
        
        // 阶段1：重型坠落（加速下落）
        tween.TweenProperty(MainSprite, "position", new Vector2(0, 10), DropDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);
        
        // 阶段2：撞击地面（压扁效果）
        tween.Chain().TweenProperty(MainSprite, "scale", 
                new Vector2(ImpactStretchX, ImpactSquashY), 0.02f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel().TweenProperty(MainSprite, "position", Vector2.Zero, 0.02f);
        
        // 阶段3：回弹恢复
        tween.Chain().TweenProperty(MainSprite, "scale", Vector2.One, RecoveryDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);
        
        return ToSignal(tween, Tween.SignalName.Finished).AsGDTask();
    }
}