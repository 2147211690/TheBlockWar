using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([],[])]
        public ApplyAnimationResult ApplyAnimation(AnimationType animation)
        {
            EnterSimulation();
            var r = obj.Steps.AddResult(new ApplyAnimationResult(CurrentDeep, animation));
            ExitSimulation();
            return r;
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