using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                bool isValid = true;
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, 0, isValid));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                if (!unit.IsNull())
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                    context.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, unit.Id, isValid));
                    state.ApplyUnitRemoveFromCoords(unit, coords, context);
                    state.ApplyUnitSetInCoords(unit, context);
                }
                else context.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, unit.Id, isValid));
            }
        }
    }
}