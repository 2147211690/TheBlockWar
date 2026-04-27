using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyDelay(int delay)
        {
            EnterSimulation();
            if (obj.Layer == SimulationLayer.Running)
                obj.Steps?.AddStep(new DelayStep(CurrentDeep, delay));
            ExitSimulation();
        }
    }
}