using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class BusyState(StatePlayer player) : State(player)
    {
        public PlayerOperation? CallOperation { get; set; }
        public State? NextState { get; set; }
        public override void OnEnter()
        {
            if (CallOperation == null || NextState == null)
            {
                StateMachine.ChangeState(Player._freeState);
                return;
            }
            Player.WaitConfirm = true;
            Player.PlayerOperationSynchronizer.PushOperation(CallOperation);
            var next = NextState;
            WaitConfirm().Forget();

            async GDTask WaitConfirm()
            {
                await GDTask.WaitUntil(() => Player.WaitConfirm == false);
                StateMachine.ChangeState(next);
            }
        }

        public override void OnExit()
        {
            CallOperation = null;
            NextState = null;
        }

    }
}