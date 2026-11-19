using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 触发一个团队当前回合的所有事件
/// </summary>
public class SkillRunPhase : TurnPhase
{
    public override string PhaseName { get; }
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Skill;
    public override int Team { get; }

    public override bool IsFocused
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            if (field)
            {
                EventScheduler.FocusedSkillTeamMask |= 1 << Team;
            }
            else
            {
                EventScheduler.FocusedSkillTeamMask &= ~(1 << Team);
            }
        }
    }
    public SkillRunPhase(int team, string phaseName)
    {
        Team = team;
        PhaseName = phaseName;
    }
    public override void OnSetOrderTable(PhaseOrderTable orderTable)
    {
        orderTable.SetSkillOrder(Team, PhaseOrder);
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
}