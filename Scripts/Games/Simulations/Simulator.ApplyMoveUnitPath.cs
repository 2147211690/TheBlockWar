using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I>? path)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (path is not {Count: > 1}) return;
                var unit = obj.State.UnitMapState.GetUnit(coords + path[0]);
                if (unit.IsNull())
                {
                    obj.Steps?.AddStep(new MoveUnitPathStep(CurrentDeep, coords, path, unit.Id, 0,
                        false));
                    return;
                }

                var targetIndex = 1;

                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(obj.State, coords + path[targetIndex]))
                        break;

                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(obj.State, coords + path[targetIndex]))
                        break;


                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new MoveUnitPathStep(CurrentDeep, coords, path, unit.Id,
                        targetIndex, true));
                    return;
                }

                obj.Steps?.AddStep(new MoveUnitPathStep(CurrentDeep, coords, path, unit.Id, targetIndex,
                    true));
                if (targetIndex > 0) obj.ApplyMoveUnit(coords + path[0], coords + path[targetIndex]);
            }
        }
    }
}