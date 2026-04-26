using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyRectWind(Rect2I rect, DirMask dir, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new RectWindStep(CurrentDeep, rect, dir));
                    return;
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
                var aims = new Queue<Vector2I>();
                for (var j = 0; j < secondOffsetCount; j++)
                {
                    for (var i = 0; i < firstOffsetCount; i++)
                    {
                        var coords = start + firstOffset * i + secondOffset * j;
                        if (state.UnitMapState.HasUnit(coords))
                            aims.Enqueue(coords);
                    }
                }
                while (aims.Count > 0)
                {
                    var coords = aims.Dequeue();
                    state.ApplyPushUnit(coords, dir, context);
                }
            }
        }
    }
}