using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyDropUnit(Vector2I coords, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                state.UnitMapState.RemoveUnit(unit.Id);
                context.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, unit.Id, true));
            }
        }
    }
}