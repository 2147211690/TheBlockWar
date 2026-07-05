using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Games.Simulations;

public abstract class AnimationLineFunc
{
    protected readonly Vector3 NormalizedStart;
    protected readonly Vector3 NormalizedEnd;
    public float Length => _length ??= GetLength();
    private readonly float _gridHeight;
    private float? _length;

    protected AnimationLineFunc(Vector3 start, Vector3 end, float gridHeight)
    {
        _gridHeight = gridHeight;
        NormalizedStart = start with { Z = start.Z / gridHeight };
        NormalizedEnd = end with { Z = end.Z / gridHeight };
    }
    protected abstract float GetLength();
    public abstract Vector3 GetPoint(float t);
    protected Vector3 Denormalize(Vector3 pos) => pos with { Z = pos.Z * _gridHeight };
}

public class LinearLineFunc(Vector3 start, Vector3 end, float gridHeight) : AnimationLineFunc(start, end, gridHeight)
{
    protected override float GetLength() => (NormalizedEnd - NormalizedStart).Length();

    public override Vector3 GetPoint(float t) => Denormalize(NormalizedStart.Lerp(NormalizedEnd, t));
}

public class ThrowLineFunc : AnimationLineFunc
{
    private readonly Vector3 _center;
    public ThrowLineFunc(Vector3 start, Vector3 end, float gridHeight, float heightRatio) : base(start, end, gridHeight)
    {
        var c = (NormalizedStart + NormalizedEnd) / 2;
        c.Z += NormalizedStart.DistanceTo(NormalizedEnd) * heightRatio;
        _center = c;
    }

    protected override float GetLength()
    {
        return NormalizedStart.DistanceTo(_center) + NormalizedEnd.DistanceTo(_center);
    }

    public override Vector3 GetPoint(float t)
    {
        var l1 = NormalizedStart.Lerp(_center, t);
        var l2 = _center.Lerp(NormalizedEnd, t);
        return Denormalize(l1.Lerp(l2, t));
    }
}

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyAnimationResult ApplyAnimation(AnimationType animation)
        {
            EnterSimulation();
            var r = obj.Steps.AddResult(new ApplyAnimationResult(CurrentDeep, CurrentTeam, animation));
            ExitSimulation();
            return r;
        }
    }
    
    public static partial void ApplyAnimationReplayCore(IGameState state, ApplyAnimationResult result)
    {
    }
    
    public static partial void ApplyAnimationViewDraw(IGameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
        if (result.Animation is ShotSomethingAnim shot)
        {
            var h = view.GetTileHeight(shot.Coords);
            var start = new Vector3(shot.Coords.X, shot.Coords.Y, h);
            var end = new Vector3(shot.To.X, shot.To.Y, h);
            view.EventTopDraw(n =>
            {
                DrawPointLine(n, view, new LinearLineFunc(start, end, view.NormalGridHeight), sim.IsFocused);
            });
        }
        else if (result.Animation is ThrowSomethingAnim thr)
        {
            var throwStart = thr.Coords.ToUnderVec3(view);
            var throwEnd = thr.To.ToUnderVec3(view);
            view.EventTopDraw(n =>
            {
                DrawPointLine(n, view, new ThrowLineFunc(throwStart, throwEnd, view.NormalGridHeight, thr.ThrowHeight), sim.IsFocused);
            });
        }
    }

    private static void DrawPointLine(ViewNode2D node, IGameView view,
        AnimationLineFunc line, bool isFocused)
    {
        if (line.Length < 0.5f) return;
        var step = 1f / line.Length * 1.2f;
        var color = Colors.Yellow with { A = isFocused ? 1f : view.CurrentStyle.WaitAlpha };
        for (var i = 0f; i <= 1f + step / 2; i += step)
        {
            var pos = line.GetPoint(i);
            node.DrawTextureCentered(view.CurrentStyle.LinePointTexture,
                view.GamePositionToNodePosition(pos), color);
        }
    }

    public static async partial GDTask ApplyAnimationViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
        if (result.Animation is ShotSomethingAnim shot)
        {
            var v = view.CurrentStyle.EffectTable.CreateEffect(view, shot.ThingName);
            if (v == null) return;
            var startPosition = view.CoordsToGamePosition(shot.Coords);
            var endPosition = view.CoordsToGamePosition(shot.To);
            v.GamePosition = startPosition;
            v.SetGameRotation(endPosition - startPosition);
            var tween = v.CreateTween();
            tween.TweenProperty(v, "GamePosition", endPosition, shot.Time);
            await tween.WaitForTween();
            v.QueueFree();
        }
        else if (result.Animation is ThrowSomethingAnim thr)
        {
            var v = view.CurrentStyle.EffectTable.CreateEffect(view, thr.ThingName);
            if (v == null) return;
            var startPosition = view.CoordsToGamePosition(thr.Coords);
            var endPosition = view.CoordsToGamePosition(thr.To);
            var line = new ThrowLineFunc(startPosition, endPosition, view.NormalGridHeight, thr.ThrowHeight);
            v.GamePosition = startPosition;
            var timer = new RecordTimer(thr.Time);
            while (timer.HasTime)
            {
                var nTime = timer.GetNormalizedTime();
                var pos = line.GetPoint(nTime);
                var nPos = line.GetPoint(nTime + 0.03f);
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