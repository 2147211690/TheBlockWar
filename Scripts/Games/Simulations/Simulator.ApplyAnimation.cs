using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyAnimationResult ApplyAnimation(AnimationType animation)
        {
            EnterSimulation();
            var r = obj.Steps.AddResult(new ApplyAnimationResult(CurrentDeep, animation));
            ExitSimulation();
            return r;
        }
    }

    public static partial void ApplyAnimationViewDraw(GameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
        if (result.Step.Animation is ShotSomethingAnim shot)
        {
            var h = view.TilesView.GetTileHeight(shot.Coords);
            var start = new Vector3(shot.Coords.X, shot.Coords.Y, h);
            var end = new Vector3(shot.To.X, shot.To.Y, h);
            view.EventsView.DrawPointLine(AnimationLine, start, end, 0.5f, Colors.Yellow);
        }
        else if (result.Step.Animation is ThrowSomethingAnim thr)
        {
            var throwStart = new Vector3(thr.Coords.X, thr.Coords.Y, view.TilesView.GetTileHeight(thr.Coords));
            var throwEnd = new Vector3(thr.To.X, thr.To.Y, view.TilesView.GetTileHeight(thr.To));
            view.EventsView.DrawPointLine(AnimationThrowLine, throwStart, throwEnd, 0.5f, Colors.Yellow);
        }
    }
    private static Vector3 AnimationLine(Vector3 v1, Vector3 v2, float arg3)
    {
        return v1.Lerp(v2, arg3);
    }
    private static Vector3 AnimationThrowLine(Vector3 v1, Vector3 v2, float v)
    {
        var center = (v1 + v2) / 2;
        center.Z += 200;
        var l1 = v1.Lerp(center, v);
        var l2 = center.Lerp(v2, v);
        return l1.Lerp(l2, v);
    }

    public static async partial GDTask ApplyAnimationViewRun(GameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
        if (result.Step.Animation is ShotSomethingAnim shot)
        {
            var v = view.CurrentStyle.EffectTable.CreateEffect(view, shot.ThingName);
            if (v == null) return;
            var startPosition = view.GetGamePosition(shot.Coords);
            var endPosition = view.GetGamePosition(shot.To);
            v.GamePosition = startPosition;
            v.SetGameRotation(endPosition - startPosition);
            var tween = v.CreateTween();
            tween.TweenProperty(v, "GamePosition", endPosition, shot.Time);
            await tween.WaitForTween();
            v.QueueFree();
        }
        else if (result.Step.Animation is ThrowSomethingAnim thr)
        {
            var v = view.CurrentStyle.EffectTable.CreateEffect(view, thr.ThingName);
            if (v == null) return;
            var startPosition = view.GetGamePosition(thr.Coords);
            var endPosition = view.GetGamePosition(thr.To);
            v.GamePosition = startPosition;
            var startTick = Time.GetTicksMsec();
            var nTime = 0f;
            while (nTime <= 1)
            {
                nTime = (Time.GetTicksMsec() - startTick) / (thr.Time * 1000f);
                var pos = AnimationThrowLine(startPosition, endPosition, nTime);
                var nPos = AnimationThrowLine(startPosition, endPosition, nTime + 0.03f);
                v.GamePosition = pos;
                v.SetGameRotation(nPos - pos);
                await GDTask.Yield();
            }
            v.GamePosition = endPosition;
            v.QueueFree();
        }
        return;
    }
}