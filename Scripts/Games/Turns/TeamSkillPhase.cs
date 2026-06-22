using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns;

public class TeamSkillPhase : ITurnPhase
{
    public int Turn { get; private set; }
    public int Team { get; }

    public bool IsFocused
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
    public int PhaseOrder { get; private set; }
    public EventScheduler EventScheduler { get; private set; }
    
    public TeamSkillPhase(int team)
    {
        Team = team;
    }

    public async GDTask Execute()
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

    public ITurnPhase Init(int turn, EventScheduler eventScheduler, int order)
    {
        PhaseOrder = order;
        EventScheduler = eventScheduler;
        Turn = turn;
        return this;
    }

    private IEnumerable<SchedulerEvent> GetTeamSkill()
    {
        return EventScheduler.GetTurnEvents(Turn).Where(e => e is SkillEvent se && se.Team == Team);
    }
}