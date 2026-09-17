using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;

namespace TheBlockWar.Scripts.Games.Turns.Phases;
/// <summary>
/// 玩家回合
/// </summary>
public class PlayerPlaceUnitPhase : PlayerPhase
{
    public PlayerPlaceUnitPhase(IPlayerOperationSynchronizer operationSynchronizer, IPlayer player) : base(operationSynchronizer, player)
    {
    }
    public override async GDTask Execute()
    {
        Player.PhaseOrder = PhaseOrder;
        Player.CurrentTurn = Turn;
        Player.WaitConfirm = false;
        Player.TurnEnter = PlayerTurnEnterEnum.PlaceUnit;
        
        Player.IsTurn = true;
        
        await foreach (var oper in OperationSynchronizer.GetAllOperationAsync())
        {
            if (oper.PlayerId != Player.Id) continue;
            Player.WaitConfirm = true;
            if (await HandleNext(oper))
            {
                Player.IsTurn = false;
                Player.WaitConfirm = false;
                break;
            }

            Player.WaitConfirm = false;
        }
    }
}