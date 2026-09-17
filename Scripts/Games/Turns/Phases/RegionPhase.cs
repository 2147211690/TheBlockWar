namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 区域回合(新框架:阶段只切换调度器的聚焦标记,收集与执行由调度器负责)
/// </summary>
public class RegionPhase : TurnPhase
{
    public override string PhaseName { get; } = "RegionPhase";
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Region;
    public override int Team { get; } = 0;

    public override bool IsFocused
    {
        get => EventScheduler.IsFocusedRegionTurn;
        set => EventScheduler.IsFocusedRegionTurn = value;
    }

    protected override void OnInit()
    {
        // 新框架:不再创建全局区域回合事件;由调度器收到 IsFocusedRegionTurn 标记后触发全量收集。
    }

    public override async GDTask Execute()
    {
        // 新框架:阶段只切换调度器标记,触发全量收集与执行。
        EventScheduler.RunRegionTurn();
        await EventScheduler.WaitCompleteRunning();
        await GDTask.Delay(800);
    }
}