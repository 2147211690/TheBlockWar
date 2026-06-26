using TheBlockWar.Scripts.Games.Events;

namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnPhase
{
    string PhaseName { get; }
    int Turn { get; }
    bool IsFocused { get; set; }
    int PhaseOrder { get; }
    IEventScheduler EventScheduler { get; }
    ITurnManager TurnManager { get; }
    GDTask Execute();
    ITurnPhase Init(ITurnManager turnManager, int turn, IEventScheduler eventScheduler, int order);
}