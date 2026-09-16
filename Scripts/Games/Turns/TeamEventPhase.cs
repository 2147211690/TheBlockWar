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
    public override string PhaseName { get; }
    public int Team { get; }
    public override bool IsFocused
    {
        get => field;
        set
        {
        }
    }
    public TeamEventPhase(int team, string phaseName)
    {
        Team = team;
        PhaseName = phaseName;
    }

    public override async GDTask Execute()
    {
        EventScheduler.RunTeamUnitSkillTurn(Team);
        await EventScheduler.WaitCompleteRunning();
        await GDTask.Delay(800);
    }

    protected override void OnInit()
    {
    }

    private List<EventState> GetTeamSkill()
    {
        return EventScheduler.Events.Values.Where(e => e.Team == Team).ToList();
    }
}