namespace TheBlockWar.Scripts.Games.Turns.Phases;

public interface ITurnPhase
{
    string PhaseName { get; }
    PhaseEnum PhaseType { get; }
    int Team { get; }
    int Turn { get; }
    bool IsFocused { get; set; }
    int PhaseOrder { get; }
    IEventScheduler EventScheduler { get; }
    ITurnManager TurnManager { get; }
    GDTask Execute();
    ITurnPhase Init(ITurnManager turnManager, int turn, IEventScheduler eventScheduler, int order);
    void OnSetOrderTable(PhaseOrderTable orderTable);
}