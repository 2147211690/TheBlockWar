using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyAnimation(AnimationType animation, SimulationResult result)
        {
            EnterSimulation();
            result.Steps?.AddStep(new AnimationStep(IsCurrentChildSimulation, animation));
            ExitSimulation();
        }
    }
}