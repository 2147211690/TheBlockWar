using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public class PvpMode : GameMode
{
    private IPlayer[] _players;
    private IPlayer _localPlayer;

    public PvpMode(IPlayer[] players, IPlayer localPlayer)
    {
        _players = players;
        _localPlayer = localPlayer;
    }

    public override void EnterTurn(TurnManager turnManager, int turn)
    {
        turnManager.EventScheduler.CreateEvent(id => new TurnEvent(id)
        {
            TurnType = TurnEvent.TypeEnum.Region,
            Turn = turn + 1,
            Priority = int.MinValue,
            Layer = SimulationLayer.Disabled,
        });
        turnManager.EventScheduler.CreateEvent(id => new TurnEvent(id)
        {
            TurnType = TurnEvent.TypeEnum.Buff,
            Turn = turn + 1,
            Priority = int.MinValue + 1,
            Layer = SimulationLayer.Disabled,
        });
    }

    public override async GDTask Turn(TurnManager turnManager, int turn)
    {
        await RunBuffRegionEventAndAddToNextTurnEvent(turnManager, turn);
        
        Logger.Info($"开始回合{turn}");
        foreach (var player in _players)
        {
            Logger.Info($"开始玩家{player.GetHashCode()}的回合");
            player.IsTurn = true;
            await GDTask.WaitUntil(() => !player.IsTurn);
            Logger.Info($"玩家{player.GetHashCode()}的回合完成");
        }
    }

    public override void ExitTurn(TurnManager turnManager, int turn)
    {
    }
}