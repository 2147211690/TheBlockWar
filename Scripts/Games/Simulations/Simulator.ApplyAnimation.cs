using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyAnimation(AnimationType animation)
        {
            EnterSimulation();
            obj.Steps?.AddStep(new AnimationStep(CurrentDeep, animation));
            ExitSimulation();
        }
    }
}