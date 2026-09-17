namespace TheBlockWar.Scripts.Games.Turns.Phases;

public abstract class TurnPhase : ITurnPhase
{
    public abstract string PhaseName { get; }
    public abstract PhaseEnum PhaseType { get; }
    public abstract int Team { get; }
    public int Turn { get; private set; }
    public abstract bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; } = null!;
    public ITurnManager TurnManager { get; private set; } = null!;
    public abstract GDTask Execute();
    public ITurnPhase Init(ITurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        TurnManager = turnManager;
        PhaseOrder = order;
        OnInit();
        return this;
    }
    protected abstract void OnInit();
}