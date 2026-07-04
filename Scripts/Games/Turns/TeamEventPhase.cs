using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 触发一个团队当前回合的所有事件
/// </summary>
public class TeamEventPhase : TurnPhase
{
    public override string PhaseName { get; } = "TeamEventPhase";
    public int Team { get; }

    public override bool IsFocused
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            foreach (var se in GetTeamSkill())
                se.Layer = field ? SimulationLayer.Focused : SimulationLayer.Waiting;
        }
    }
    public TeamEventPhase(int team)
    {
        Team = team;
    }

    public override async GDTask Execute()
    {
        foreach (var se in GetTeamSkill())
        {
            if (!se.CheckIsValid(EventScheduler.GameState)) continue;
            se.Layer = SimulationLayer.Focused;
            await GDTask.Delay(800);
            se.Layer = SimulationLayer.Running;
            await EventScheduler.WaitCompleteRunning();
        }
    }

    protected override void OnInit()
    {
    }

    private IEnumerable<SchedulerEvent> GetTeamSkill()
    {
        return EventScheduler.GetTurnEvents(Turn).Where(e => e.Team == Team);
    }
}