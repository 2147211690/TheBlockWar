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
    public TurnManager(EventScheduler eventScheduler, IGameMode gameMode)
    {
        EventScheduler = eventScheduler;
        GameMode = gameMode;
    }

    public EventScheduler EventScheduler { get; }
    public IGameMode GameMode { get; }
    public int CurrentTurn { get => EventScheduler.CurrentTurn; set => EventScheduler.CurrentTurn = value; }
    public GameStateEnum State => GameMode.State;
    public IReadOnlyGameState GameState => EventScheduler.GameState!;
    private bool _stop;
    private int _currentFocusedTurn = -1;
    public async GDTask Start()
    {
        while (!_stop)
        {
            Logger.Info($"开始回合{CurrentTurn}");
            await RunTurnEvents(CurrentTurn);
            
            GameMode.EnterTurn(this, CurrentTurn);
            await GameMode.Turn(this, CurrentTurn);
            GameMode.ExitTurn(this, CurrentTurn);
            
            Logger.Info($"回合{CurrentTurn}完成");
            CurrentTurn++;
        }
    }
    private async GDTask RunTurnEvents(int turn)
    {
        var turnEvents = EventScheduler.GetTurnEvents(turn);
        foreach (var ev in turnEvents)
        {
            if (!ev.CheckIsValid(EventScheduler)) continue;
            ev.Layer = SimulationLayer.Focused;
            await GDTask.Delay(800);
            ev.Layer = SimulationLayer.Running;
            await EventScheduler.WaitCompleteRunning();
        }
    }
    
    public void FocusTurnEvent(int turn)
    {
        if (_currentFocusedTurn == turn) return;
        EventScheduler.BlockUpdate();
        var prev = EventScheduler.GetTurnEvents(_currentFocusedTurn);
        foreach (var ev in prev)
            ev.Layer = ev is SkillEvent skillEvent ? SimulationLayer.Waiting : SimulationLayer.Disabled;
        _currentFocusedTurn = turn;
        var now = EventScheduler.GetTurnEvents(_currentFocusedTurn);
        foreach (var ev in now)
            ev.Layer = SimulationLayer.Focused;
        EventScheduler.UnblockUpdate();
    }
    public void Stop()
    {
        _stop = true;
    }
}