using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public sealed class ActionEvent(long id) : SchedulerEvent(id)
{
    [JsonIgnore]
    public Action<SchedulerEvent, SimObject>? ActionCore { get; set => SetField(ref field, value); }
    [JsonIgnore]
    public Func<bool>? IsValidCore { get; set => SetField(ref field, value); }
    public override void OnApply(SchedulerEvent root, SimObject obj)
    {
        ActionCore?.Invoke(this, obj);
    }

    public override bool CheckIsValid(EventScheduler scheduler)
    {
        return IsValidCore?.Invoke() ?? true;
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> ActionEvent";
    }
}