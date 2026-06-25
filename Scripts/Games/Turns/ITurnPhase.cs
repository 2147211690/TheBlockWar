using TheBlockWar.Scripts.Games.Events;

namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnPhase
{
    int Turn { get; }
    bool IsFocused { get; set; }
    int PhaseOrder { get; }
    IEventScheduler EventScheduler { get; }
    TurnManager TurnManager { get; }
    GDTask Execute();
    ITurnPhase Init(TurnManager turnManager, int turn, IEventScheduler eventScheduler, int order);
}