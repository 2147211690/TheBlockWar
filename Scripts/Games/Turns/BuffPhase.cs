namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 处理 buff 的回合(新框架:阶段只切换调度器的聚焦标记,收集与执行由调度器负责)
/// </summary>
public class BuffPhase : TurnPhase
{
    public override string PhaseName { get; } = "BuffPhase";

    public override bool IsFocused
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            // 聚焦/失焦时,让调度器把整类 buff 触发者标记为 Focused/Running
            EventScheduler.IsFocusedBuffTurn = value;
        }
    }

    protected override void OnInit()
    {
        // 新框架:不再创建全局 buff 回合事件;由调度器收到 IsFocusedBuffTurn 标记后触发全量收集。
    }

    public override async GDTask Execute()
    {
        // 新框架:阶段只切换调度器标记,触发全量收集与执行。
        EventScheduler.RunBuffTurn();
        await EventScheduler.WaitCompleteRunning();
        await GDTask.Delay(800);
    }
}