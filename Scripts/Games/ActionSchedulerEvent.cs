using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class ActionSchedulerEvent : SchedulerEvent
{
    public ActionSchedulerEvent(Action<(SchedulerEvent Root, IGameState State, SimulationContext Result)>? applyAction = null)
    {
        ApplyAction = applyAction;
    }

    public Action<(SchedulerEvent Root, IGameState State, SimulationContext Result)>? ApplyAction
    { get; set => SetField(ref field, value, false); }
    public override void OnApply(SchedulerEvent root, IGameState state, SimulationContext context)
    {
        ApplyAction?.Invoke((root, state, context));
    }
}