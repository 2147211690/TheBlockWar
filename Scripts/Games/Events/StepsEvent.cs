using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public class StepsEvent(long id) : SchedulerEvent(id)
{
    public List<ApplyStep>? Steps { get; set => SetField(ref field, value, false); }
    public int StartDelay { get; set => SetField(ref field, value, false); } = 0;
    public int EndDelay { get; set => SetField(ref field, value, false); } = 0;
    public StepsRunningMode RunningMode { get; set => SetField(ref field, value, false); }
    
    public override void OnApply(SchedulerEvent root, SimObject obj)
    {
        if (Steps is null) return;
        obj.ApplySteps(Steps, StartDelay, EndDelay, RunningMode);
    }

    public override string ToString()
    {
        return $"{base.ToString()} -> StepsEvent 步骤数:{Steps?.Count ?? 0}";
    }
}