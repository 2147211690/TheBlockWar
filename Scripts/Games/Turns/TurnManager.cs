using System.Linq;
using TheBlockWar.Scripts.Games.Turns.Phases;

namespace TheBlockWar.Scripts.Games.Turns;

public class TurnManager : ITurnManager
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public TurnManager(IEventScheduler eventScheduler, ITurnStrategy turnStrategy)
    {
        EventScheduler = eventScheduler;
        TurnStrategy = turnStrategy;
    }
    public IEventScheduler EventScheduler { get; }
    public ITurnStrategy TurnStrategy { get; }
    public int CurrentTurn { get => EventScheduler.CurrentTurn; set => EventScheduler.CurrentTurn = value; }
    public IReadOnlyList<ITurnPhase> CurrentPhases => _currentPhases;
    public IReadOnlyList<ITurnPhase> NextPhases => _nextPhases;
    public event EventHandler<(int index, bool isEnter, ITurnPhase phase)> PhaseChanged;
    
    private List<ITurnPhase> _currentPhases = new();
    private List<ITurnPhase> _nextPhases = new();
    
    private bool _stop;
    public async GDTask Start()
    {
        MoveNextPhases();
        while (!_stop)
        {
            for (var index = 0; index < _currentPhases.Count; index++)
            {
                var phase = _currentPhases[index];
                PhaseChanged?.Invoke(this, (index, true, phase));
                await phase.Execute();
                PhaseChanged?.Invoke(this, (index, false, phase));
            }
            MoveNextPhases();
        }
    }

    private void MoveNextPhases()
    {
        CurrentTurn++;
        if (CurrentTurn == 0)
        {
            _currentPhases.AddRange(GetTurnPhases(CurrentTurn));
            EventScheduler.SetCurrentTurnPhaseOrder(_currentPhases);
            _nextPhases.AddRange(GetTurnPhases(CurrentTurn + 1));
            EventScheduler.SetNextTurnPhaseOrder(_nextPhases);
            SetPhaseOrders();
            return;
        }
        _currentPhases.Clear();
        EventScheduler.PhaseOrders.Remove(CurrentTurn - 1);
        _currentPhases.AddRange(_nextPhases);
        EventScheduler.SetCurrentTurnPhaseOrder(_currentPhases);
        
        _nextPhases.Clear();
        _nextPhases.AddRange(GetTurnPhases(CurrentTurn + 1));
        EventScheduler.SetNextTurnPhaseOrder(_nextPhases);
        SetPhaseOrders();
    }

    private void SetPhaseOrders()
    {
        EventScheduler.PhaseOrders[CurrentTurn] =
            _currentPhases.Select(a => (a.PhaseName, a.PhaseOrder)).ToDictionary();
        EventScheduler.PhaseOrders[CurrentTurn + 1] =
            _nextPhases.Select(a => (a.PhaseName, a.PhaseOrder)).ToDictionary();
    }

    private IEnumerable<ITurnPhase> GetTurnPhases(int turn)
    {
        return TurnStrategy.GetTurnPhases(this, turn).Select((p, i) => p.Init(this, turn, EventScheduler, i));
    }
    public void FocusNextTurn(bool focused)
    {
        EventScheduler.BlockUpdate();
        foreach (var phase in _nextPhases) phase.IsFocused = focused;
        EventScheduler.UnblockUpdate();
    }
    public void Stop()
    {
        _stop = true;
    }


}