using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Modes;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using Wintellect.PowerCollections;

namespace TheBlockWar.Scripts.Games.Turns;

public class TurnManager
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public TurnManager(EventScheduler eventScheduler, ITurnStrategy turnStrategy)
    {
        EventScheduler = eventScheduler;
        TurnStrategy = turnStrategy;
    }
    public EventScheduler EventScheduler { get; }
    public ITurnStrategy TurnStrategy { get; }
    public int CurrentTurn { get => EventScheduler.CurrentTurn; set => EventScheduler.CurrentTurn = value; }
    
    private List<ITurnPhase> _currentPhases = new();
    private List<ITurnPhase> _nextPhases = new();
    
    private bool _stop;
    public async GDTask Start()
    {
        MoveNextPhases();
        while (!_stop)
        {
            foreach (var phase in _currentPhases)
            {
                Logger.Debug("阶段进入{0}", phase.GetType().Name);
                await phase.Execute();
                Logger.Debug("阶段退出{0}", phase.GetType().Name);
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
        (_currentPhases, _nextPhases) = (_nextPhases, _currentPhases);
        _nextPhases.Clear();
        _nextPhases.AddRange(GetTurnPhases(CurrentTurn + 1));
    }

    private IEnumerable<ITurnPhase> GetTurnPhases(int turn)
    {
        return TurnStrategy.GetTurnPhases(this, turn).Select((p, i) => p.Init(turn, EventScheduler, i));
    }
    public void FocusNextTurn(int turn)
    {
        foreach (var phase in _nextPhases) phase.IsFocused = turn >= 0;
    }
    public void Stop()
    {
        _stop = true;
    }
}