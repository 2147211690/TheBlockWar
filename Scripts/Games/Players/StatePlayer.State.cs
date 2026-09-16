using System.Diagnostics.CodeAnalysis;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;
public partial class StatePlayer
{
    private abstract class State(StatePlayer player) : IState
    {
        protected StatePlayer Player = player;
        protected Logger Logger => StatePlayer.Logger;
        public IReadOnlyGameState? GameState => Player.GameScheduler?.GameState;
        public IEventScheduler? EventScheduler => Player.GameScheduler;
        public StateMachine<State> StateMachine => Player._stateMachine;

        public Vector2I? FocusEventCoords
        {
            get => field;
            set
            {
                if (field == value) return;
                field = value;
                // 新框架:设置调度器的关注坐标触发全量收集,收集器自动把该坐标的区域触发者标记为 Focused
                EventScheduler!.FocusedCoords = value;
            }
        }

        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }
}