using TheBlockWar.Scripts.Games.GameStates;
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
        public long FocusedUnitId => Player.FocusedUnitId;
        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }
}