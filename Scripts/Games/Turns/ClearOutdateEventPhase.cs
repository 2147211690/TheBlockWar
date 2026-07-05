using System.Linq;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 用来清除当前和之前回合的所有事件
/// </summary>
public class ClearOutdateEventPhase : TurnPhase
{
    public override string PhaseName { get; } = "ClearOutdateEventPhase";
    public override bool IsFocused { get; set; }
    public override GDTask Execute()
    {
        EventScheduler.BlockUpdate();
        foreach (var a in EventScheduler.Events.Values.Where(a => a.Turn <= Turn).ToList())
        {
            EventScheduler.RemoveEvent(a.Id);
        }
        EventScheduler.UnblockUpdate();
        return GDTask.CompletedTask;
    }
    protected override void OnInit()
    {
    }
}