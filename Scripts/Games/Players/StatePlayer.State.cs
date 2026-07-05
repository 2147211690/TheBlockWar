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

        private IReadOnlyList<EventState>? _focusEvents;
        public Vector2I? FocusEventCoords
        {
            get => field;
            set
            {
                if (field == value) return;
                _focusEvents.SetLayer(SimulationLayer.Waiting);
                field = value;
                _focusEvents = field == null ? null : EventScheduler!.GetCoordsEvents(field.Value);
                _focusEvents.SetLayer(SimulationLayer.Focused);
            }
        }

        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }
}