using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public void ApplyAnimation(AnimationType animation)
        {
            EnterSimulation();
            obj.Steps?.AddResult(new ApplyAnimationResult(CurrentDeep, animation));
            ExitSimulation();
        }
    }

    public static partial void ApplyAnimationViewDraw(GameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
    }

    public static partial GDTask ApplyAnimationViewRun(GameView view, ReadOnlySimObject sim, ApplyAnimationResult result)
    {
        return view.AnimationView.Play(result.Step.Animation);
    }
}