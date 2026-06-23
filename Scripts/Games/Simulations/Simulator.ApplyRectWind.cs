using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyRectWindResult ApplyRectWind(Rect2I rect, DirMask dir)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyRectWindResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyRectWindResult(
                    CurrentDeep, CurrentTeam, rect, dir));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }
                Vector2I start = dir switch
                {
                    DirMask.Up => new (rect.Position.X, rect.Position.Y + rect.Size.Y),
                    DirMask.Down => rect.Position,
                    DirMask.Left => new (rect.Position.X + rect.Size.X, rect.Position.Y),
                    DirMask.Right => rect.Position,
                    _ => rect.Position,
                };
                Vector2I firstOffset = dir switch
                {
                    DirMask.Up => Vector2I.Right,
                    DirMask.Down => Vector2I.Right,
                    DirMask.Left => Vector2I.Down,
                    DirMask.Right => Vector2I.Down,
                    _ => Vector2I.Right,
                };
                int firstOffsetCount = dir switch
                {
                    DirMask.Up => rect.Size.X,
                    DirMask.Down => rect.Size.X,
                    DirMask.Left => rect.Size.Y,
                    DirMask.Right => rect.Size.Y,
                    _ => 0,
                };
                Vector2I secondOffset = dir.ToVec2I();
                int secondOffsetCount = dir switch
                {
                    DirMask.Up => rect.Size.Y,
                    DirMask.Down => rect.Size.Y,
                    DirMask.Left => rect.Size.X,
                    DirMask.Right => rect.Size.X,
                    _ => 0,
                };
                var aims = new Queue<Queue<Vector2I>>();
                var currentLine = new Queue<Vector2I>();
                for (var j = 0; j < secondOffsetCount; j++)
                {
                    for (var i = 0; i < firstOffsetCount; i++)
                    {
                        var coords = start + firstOffset * i + secondOffset * j;
                        if (obj.State.UnitMapState.HasUnit(coords))
                        {
                            currentLine.Enqueue(coords);
                        }
                    }

                    if (currentLine.Count > 0)
                    {
                        aims.Enqueue(currentLine);
                        currentLine = new Queue<Vector2I>();
                    }
                }
                while (aims.Count > 0)
                {
                    var line = aims.Dequeue();
                    obj.BeginSteps(0, 60, StepsRunningMode.ParallelWithoutWait);
                    while (line.Count > 0)
                    {
                        var coord = line.Dequeue();
                        obj.ApplyPushUnit(coord, dir);
                    }
                    obj.EndSteps();
                }
                return r;
            }
        }
    }
    public static partial void ApplyRectWindReplayCore(IGameState state, ApplyRectWindResult result)
    {
    }
    public static partial void ApplyRectWindViewDraw(IGameView view, ReadOnlySimObject sim, ApplyRectWindResult result)
    {
    }
    
    public static partial GDTask ApplyRectWindViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyRectWindResult result)
    {
        return GDTask.CompletedTask;
    }
}