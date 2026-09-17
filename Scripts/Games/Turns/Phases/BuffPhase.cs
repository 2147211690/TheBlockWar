namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 处理 buff 的回合(新框架:阶段只切换调度器的聚焦标记,收集与执行由调度器负责)
/// </summary>
public class BuffPhase : TurnPhase
{
    public override string PhaseName { get; } = "BuffPhase";
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Buff;
    public override int Team { get; } = 0;

    public override bool IsFocused
    {
        get => EventScheduler.IsFocusedBuffTurn;
        set => EventScheduler.IsFocusedBuffTurn = value;
    }

    protected override void OnInit()
    {
    }

    public override async GDTask Execute()
    {
        EventScheduler.RunBuffTurn();
        await EventScheduler.WaitCompleteRunning();
        await GDTask.Delay(800);
    }
}