using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public class StepsEvent(long id) : SchedulerEvent(id)
{
    public ApplyStep[]? Steps { get; set => SetField(ref field, value, false); }
    public override void OnApply(SchedulerEvent root, IGameState state, SimulationContext context)
    {
        if (Steps is null) return;
        state.ApplySteps(Steps, context);
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> StepsEvent 步骤数:{Steps?.Length ?? 0}";
    }
}