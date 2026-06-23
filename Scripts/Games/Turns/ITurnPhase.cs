using TheBlockWar.Scripts.Games.Events;

namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnPhase
{
    int Turn { get; }
    bool IsFocused { get; set; }
    int PhaseOrder { get; }
    IEventScheduler EventScheduler { get; }
    GDTask Execute();
    ITurnPhase Init(int turn, IEventScheduler eventScheduler, int order);
}