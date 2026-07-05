using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 触发一个团队当前回合的所有事件
/// </summary>
public class TeamEventPhase : TurnPhase
{
    public override string PhaseName { get; } = "TeamEventPhase";
    public int Team { get; }
    private List<EventState>? _teamSkillEvents;
    public override bool IsFocused
    {
        get => field;
        set
        {
            if (field == value) return;
            _teamSkillEvents?.SetLayer(field ? SimulationLayer.Waiting : SimulationLayer.Disabled);
            field = value;
            _teamSkillEvents = GetTeamSkill();
            _teamSkillEvents?.SetLayer(field ? SimulationLayer.Focused : SimulationLayer.Waiting);
        }
    }
    public TeamEventPhase(int team)
    {
        Team = team;
    }

    public override async GDTask Execute()
    {
        var teamSkillEvents = GetTeamSkill();
        foreach (var se in teamSkillEvents)
        {
            if (!se.CheckIsValid(EventScheduler.GameState)) continue;
            se.Layer = SimulationLayer.Focused;
            await GDTask.Delay(800);
            se.Layer = SimulationLayer.Running;
            await EventScheduler.WaitCompleteRunning();
            await GDTask.Delay(100);
        }
    }

    protected override void OnInit()
    {
    }

    private List<EventState> GetTeamSkill()
    {
        return EventScheduler.GetTurnEvents(Turn).Where(e => e.Team == Team).ToList();
    }
}