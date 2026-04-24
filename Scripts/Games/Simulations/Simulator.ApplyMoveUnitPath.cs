using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I>? path, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (path is not {Count: > 1}) return;
                var unit = state.UnitMapState.GetUnit(coords + path[0]);
                if (unit.IsNull())
                {
                    context.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, 0,
                        false));
                    return;
                }

                var targetIndex = 1;

                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(state, coords + path[targetIndex]))
                        break;

                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(state, coords + path[targetIndex]))
                        break;


                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id,
                        targetIndex, true));
                    return;
                }

                context.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, targetIndex,
                    true));
                if (targetIndex > 0) state.ApplyMoveUnit(coords + path[0], coords + path[targetIndex], context);
            }
        }
    }
}