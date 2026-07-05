using System.Linq;

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
            _nextPhases.AddRange(GetTurnPhases(CurrentTurn + 1));
            return;
        }
        _currentPhases.Clear();
        _currentPhases.AddRange(_nextPhases);
        _nextPhases.Clear();
        _nextPhases.AddRange(GetTurnPhases(CurrentTurn + 1));
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