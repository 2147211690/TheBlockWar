using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyAnimation(int delay, SimulationResult result)
        {
            EnterSimulation();
            if (result.Layer == SimulationLayer.Running)
                result.Steps?.AddStep(new DelayStep(IsCurrentChildSimulation, delay));
            ExitSimulation();
        }
    }
}