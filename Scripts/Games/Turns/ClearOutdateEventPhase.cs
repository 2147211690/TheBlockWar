using System.Linq;

namespace TheBlockWar.Scripts.Games.Turns;

public class ClearOutdateEventPhase : ITurnPhase
{
    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public GDTask Execute()
    {
        foreach (var a in EventScheduler.Events.Values.Where(a => a.Turn <= Turn).ToList())
        {
            EventScheduler.DeleteEvent(a);
        }
        return GDTask.CompletedTask;
    }

    public ITurnPhase Init(TurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        return this;
    }
}