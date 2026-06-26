namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnManager
{
    IEventScheduler EventScheduler { get; }
    ITurnStrategy TurnStrategy { get; }
    int CurrentTurn { get; set; }
    GDTask Start();
    void FocusNextTurn(bool focused);
    void Stop();
    IReadOnlyList<ITurnPhase> CurrentPhases { get; }
    IReadOnlyList<ITurnPhase> NextPhases { get; }
    event EventHandler<(int index, bool isEnter, ITurnPhase phase)> PhaseChanged;
}