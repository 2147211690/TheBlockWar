using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;
public partial class StatePlayer
{
    private abstract class State(StatePlayer player) : IState
    {
        protected StatePlayer _player = player;
        protected Logger Logger => StatePlayer.Logger;
        public IReadOnlyGameState? GameState => _player.GameScheduler?.GameState;
        public GameScheduler? GameScheduler => _player.GameScheduler;
        public StateMachine<State> StateMachine => _player._stateMachine;
        public long FocusedUnitId => _player._freeState.FocusedUnitId;
        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }
}