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
                Focus(field, SimulationLayer.Waiting);
                field = value;
                Focus(field, SimulationLayer.Focused);
                
                void Focus(in Vector2I? focusEventCoordsField, SimulationLayer layer)
                {
                    if (focusEventCoordsField != null)
                    {
                        var prevEvents = EventScheduler.GetCoordsEvents(focusEventCoordsField.Value);
                        EventScheduler.BlockUpdate();
                        foreach (var e in prevEvents)
                        {
                            e.Layer = layer;
                        }
                        EventScheduler.UnblockUpdate();
                    }
                }
            }
        }

        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }
}