using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 重置单位(技能使用次数)回合
/// </summary>
public class ResetUnitPhase : TurnPhase
{
    public override string PhaseName { get; } = "ResetUnitPhase";
    public override PhaseEnum PhaseType { get; } = PhaseEnum.ResetUnit;
    public override int Team { get; } = 0;
    public override bool IsFocused { get; set; }

    public override GDTask Execute()
    {
        EventScheduler.RunAction(obj =>
        {
            obj.ApplyResetUnit();
        });
        return GDTask.CompletedTask;
    }

    protected override void OnInit()
    {
    }
}