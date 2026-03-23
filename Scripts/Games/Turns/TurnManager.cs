using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Modes;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Turns;

public class TurnManager
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public TurnManager(GameScheduler gameScheduler, GameView gameView, IGameMode gameMode)
    {
        GameScheduler = gameScheduler;
        GameMode = gameMode;
        GameView = gameView;
    }

    public GameScheduler GameScheduler { get; }
    public IGameMode GameMode { get; }
    public GameView GameView { get; }
    public int CurrentTurn { get; private set; }
    public GameStateEnum State => GameMode.State;
    public IReadOnlyGameState GameState => GameScheduler.GameState!;
    private bool _stop;
    public async GDTask Start()
    {
        while (!_stop)
        {
            Logger.Info($"开始回合{CurrentTurn}");
            GameMode.EnterTurn(this, CurrentTurn);
            await GameMode.Turn(this, CurrentTurn);
            GameMode.ExitTurn(this, CurrentTurn);
            Logger.Info($"回合{CurrentTurn}完成");
            CurrentTurn++;
            await GDTask.Delay(100);
        }
    }

    public void Stop()
    {
        _stop = true;
    }
}