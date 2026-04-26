using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplySteps(ApplyStep[] applys, SimulationContext context)
        {
            foreach (var apply in applys)
            {
                apply.Apply(state, context);
            }
        }
    }
}