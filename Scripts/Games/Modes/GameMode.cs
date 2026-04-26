using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public abstract class GameMode : IGameMode
{
    protected static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GameMode(){}
    public GameStateEnum State { get; protected set; } = GameStateEnum.Paused;
    
    public abstract void EnterTurn(TurnManager turnManager, int turn);

    public abstract GDTask Turn(TurnManager turnManager, int turn);

    public abstract void ExitTurn(TurnManager turnManager, int turn);
    
    protected async GDTask RunBuffRegionEventAndAddToNextTurnEvent(TurnManager turnManager, int turn, int delay = 800)
    {

    }
    
}