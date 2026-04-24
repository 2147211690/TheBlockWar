using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyAnimation(AnimationType animation, SimulationContext context)
        {
            EnterSimulation();
            context.Steps?.AddStep(new AnimationStep(IsCurrentChildSimulation, animation));
            ExitSimulation();
        }
    }
}