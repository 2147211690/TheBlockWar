using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                bool isValid = true;
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, 0, isValid));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                if (!unit.IsNull())
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                    result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit.Id, isValid));
                    state.ApplyUnitRemoveFromCoords(unit, coords, result);
                    state.ApplyUnitSetInCoords(unit, result);
                }
                else result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit.Id, isValid));
            }
        }
    }
}