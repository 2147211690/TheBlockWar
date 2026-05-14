using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class AnimationView : Node2D
{
    [Export] public GameView GameView { get; set; } = null!;
    [Export] public Godot.Collections.Dictionary<string, Texture2D> Somethings { get; set; } = null!;
    public GDTask Play(AnimationType step)
    {
        switch (step)
        {
            case ShotSomethingAnim shotSomethingAnim:
                return PlayShotSomethingAnim(shotSomethingAnim); 
            case ThrowSomethingAnim throwSomethingAnim:
                return PlayThrowSomethingAnim(throwSomethingAnim);
            case null:
                break;
        }
        return GDTask.CompletedTask;
    }

    private async GDTask PlayThrowSomethingAnim(ThrowSomethingAnim anim)
    {
        if (!Somethings.TryGetValue(anim.ThingName, out var texture)) return; 
        var sprite = PopSomething(anim.Coords, texture);
        var startPos = sprite.Position;
        
        var endPos = GameView.TilesView.GetRenderPosition(anim.To, Layer. Underground , 1); 

        var throwStart = GameView.TilesView.GetRealPosition(anim.Coords, Layer.Underground);
        var throwEnd = GameView.TilesView.GetRealPosition(anim.To, Layer.Underground);
        var throwCenter = throwEnd.Lerp(throwStart, 0.5f) + new Vector2(0, -anim.ThrowHeight);
        
        var startTime = Time.GetTicksMsec();
        while (Time.GetTicksMsec() - startTime < anim.Time * 1000)
        {
            var p = (Time.GetTicksMsec() - startTime) / (anim.Time * 1000);
            var s1 = throwStart.Lerp(throwCenter, p);
            var s2 = throwCenter.Lerp(throwEnd, p);
            var s3 = s1.Lerp(s2, p);
            
            sprite.Position = startPos.Lerp(endPos, p);
            sprite.Offset = s3 - sprite.Position;
            await GDTask.Yield();
        }
        
        PushSomething(sprite);
    }

    private async GDTask PlayShotSomethingAnim(ShotSomethingAnim anim) 
    { 
        if (!Somethings.TryGetValue(anim.ThingName, out var texture)) return; 
        var sprite = PopSomething(anim.Coords, texture);
        var endPos = GameView.TilesView.GetRenderPosition(anim.To, Layer. Underground , 1); 
        var endOffset = GameView.TilesView.GetRenderOffset(anim.To, Layer. Underground , 1); 
        
        var tween = sprite.CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic);
        
        tween.TweenProperty(sprite, "position", endPos, anim.Time);
        tween.Parallel();
        tween.TweenProperty(sprite, "offset", endOffset, anim.Time);
        await tween.WaitForTween(); 
        
        PushSomething(sprite);
    }

    private void PushSomething(Sprite2D sprite)
    {
        sprite.Texture = null; 
        sprite.Offset = Vector2.Zero; 
        sprite.Scale = Vector2.One;
        sprite.Rotation = 0;
        GameView.ViewPools.Sprite2DPool.Return(sprite);
    }

    private Sprite2D PopSomething(Vector2I coords, Texture2D texture)
    {
        var sprite = GameView.ViewPools.Sprite2DPool.Get(); 
        sprite.Texture = texture; 
        sprite.Position = GameView.TilesView.GetRenderPosition(coords, Layer. Underground , 1); 
        sprite.Offset = GameView.TilesView.GetRenderOffset(coords, Layer. Underground , 1);
        return sprite;
    }
}