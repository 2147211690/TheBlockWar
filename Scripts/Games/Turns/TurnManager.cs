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
    public TurnManager(GameScheduler gameScheduler, GameView gameView, IGameMode gameMode)
    {
        GameScheduler = gameScheduler;
        GameMode = gameMode;
        _turnEventManager = new TurnEventManager(OnTurnEventEntered);
        GameView = gameView;
    }

    public GameScheduler GameScheduler { get; }
    public IGameMode GameMode { get; }
    public GameView GameView { get; }
    public int CurrentTurn { get; private set; }
    public GameStateEnum State => GameMode.State;
    public IReadOnlyGameState GameState => GameScheduler.GameState!;
    private bool _stop;
    private TurnEventManager _turnEventManager;
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
        foreach (var ev in _turnEventManager.GetTurnNormalEvents(turn))
        {
            GameScheduler.SetNextCompleteDelay(800);
            GameScheduler.AddEvent(ev, SimulationLayer.Focused);
            GameScheduler.SetNextCompleteDelay(100);
            ev.Layer = SimulationLayer.Running;
        }
        _turnEventManager.RemoveTurnNormalEvents(turn);
        foreach (var ev in _turnEventManager.GetTurnSkillEvents(turn))
        {
            GameScheduler.SetNextCompleteDelay(800);
            GameScheduler.AddEvent(ev, SimulationLayer.Focused);
            GameScheduler.SetNextCompleteDelay(100);
            ev.Layer = SimulationLayer.Running;
        }
        _turnEventManager.RemoveTurnSkillEvents(turn);
    }
    
    private bool OnTurnEventEntered(SchedulerEvent ev)
    {
        if (_currentFocusedTurn != -1) throw new InvalidOperationException("正在聚焦回合事件，不能添加回合事件");
        if (ev.Turn <= CurrentTurn)
        {
            GameScheduler.AddEvent(ev, SimulationLayer.Running);
            return false;
        }
        if (ev is SkillSchedulerEvent skillEv) GameScheduler.AddEvent(skillEv, SimulationLayer.Waiting);
        else GameScheduler.RemoveEvent(ev);
        return true;
    }
    public void AddTurnEvent(SchedulerEvent ev)
    {
        _turnEventManager.AddTurnEvent(ev);
    }
    public void FocusTurnEvent(int turn)
    {
        if (_currentFocusedTurn == turn) return;
        GameScheduler.BlockUpdate();
        GameScheduler.RemoveEventMany(_turnEventManager.GetTurnNormalEvents(_currentFocusedTurn));
        foreach(var ev in _turnEventManager.GetTurnSkillEvents(_currentFocusedTurn)) ev.Layer = SimulationLayer.Waiting;
        
        _currentFocusedTurn = turn;
        
        if (_currentFocusedTurn == -1)
        {
            GameScheduler.UnblockUpdate();
            return;
        }
        GameScheduler.AddEventMany(_turnEventManager.GetTurnNormalEvents(turn), SimulationLayer.Focused);
        foreach(var ev in _turnEventManager.GetTurnSkillEvents(turn)) ev.Layer = SimulationLayer.Focused;
        GameScheduler.UnblockUpdate();
    }
    public void Stop()
    {
        _stop = true;
    }
}