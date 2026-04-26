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
    public TurnManager(EventScheduler eventScheduler, GameView gameView, IGameMode gameMode)
    {
        EventScheduler = eventScheduler;
        GameMode = gameMode;
        GameView = gameView;
    }

    public EventScheduler EventScheduler { get; }
    public IGameMode GameMode { get; }
    public GameView GameView { get; }
    public int CurrentTurn { get; private set; }
    public GameStateEnum State => GameMode.State;
    public IReadOnlyGameState GameState => EventScheduler.GameState!;
    private bool _stop;
    private int _currentFocusedTurn = -1;
    public async GDTask Start()
    {
        while (!_stop)
        {
            Logger.Info($"开始回合{CurrentTurn}");
            RunTurnEvents(CurrentTurn);
            
            GameMode.EnterTurn(this, CurrentTurn);
            await GameMode.Turn(this, CurrentTurn);
            GameMode.ExitTurn(this, CurrentTurn);
            
            Logger.Info($"回合{CurrentTurn}完成");
            CurrentTurn++;
        }
    }
    private void RunTurnEvents(int turn)
    {
        var turnEvents = EventScheduler.GetTurnEvents(turn);
        foreach (var ev in turnEvents)
        {
            Logger.Debug($"运行回合事件开始一个");
            EventScheduler.SetNextCompleteDelay(800);
            Logger.Debug($"Focused");
            ev.Layer = SimulationLayer.Focused;
            EventScheduler.SetNextCompleteDelay(100);
            Logger.Debug($"Running");
            ev.Layer = SimulationLayer.Running;
            Logger.Debug($"运行回合事件结束一个");
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