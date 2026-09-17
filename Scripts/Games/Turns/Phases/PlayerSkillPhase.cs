using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 触发一个团队当前回合的所有事件
/// </summary>
public class PlayerSkillPhase : TurnPhase
{
    public override string PhaseName { get; }
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Skill;
    public override int Team { get; }

    public override bool IsFocused
    {
        get => EventScheduler.IsFocusedSkillTurn;
        set
        { 
            EventScheduler.IsFocusedSkillTurn = value;
        }
    }
    public PlayerSkillPhase(int team, string phaseName)
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
}