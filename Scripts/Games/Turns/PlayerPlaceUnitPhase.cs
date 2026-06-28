using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 玩家回合
/// </summary>
public class PlayerPlaceUnitPhase : PlayerPhase
{
    public PlayerPlaceUnitPhase(IPlayerOperationSynchronizer operationSynchronizer, IPlayer player) : base(operationSynchronizer, player)
    {
    }
    public override GDTask Execute()
    {
        Player.TurnEnter = PlayerTurnEnterEnum.PlaceUnit;
        return base.Execute();
    }
}