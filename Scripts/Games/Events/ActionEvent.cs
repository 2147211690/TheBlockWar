using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class ActionEvent(long id) : SchedulerEvent(id)
{
    [JsonIgnore]
    public Action<SchedulerEvent, IGameState, SimulationContext>? ActionCore { get; set => SetField(ref field, value); }
    public override void OnApply(SchedulerEvent root, IGameState state, SimulationContext context)
    {
        ActionCore?.Invoke(this, state, context);
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> ActionEvent";
    }
}