using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class ActionSchedulerEvent : SchedulerEvent
{
    public ActionSchedulerEvent(Action<(SchedulerEvent Root, IGameState State, SimulationResult Result)>? applyAction = null)
    {
        ApplyAction = applyAction;
    }

    public Action<(SchedulerEvent Root, IGameState State, SimulationResult Result)>? ApplyAction
    { get; set => SetField(ref field, value, false); }
    public override void OnApply(SchedulerEvent root, IGameState state, SimulationResult result)
    {
        ApplyAction?.Invoke((root, state, result));
    }
}