using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyRemoveEventResult ApplyRemoveEvent(long eventId)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyRemoveEventResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyRemoveEventResult(
                    CurrentDeep, CurrentTeam, eventId));
                if (obj.Layer == SimulationLayer.Waiting) return r;
                obj.State.EventManagerState.RemoveEvent(eventId);
                return r;
            }
        }
    }
    public static partial void ApplyRemoveEventReplayCore(IGameState state, ApplyRemoveEventResult result)
    {
        state.EventManagerState.RemoveEvent(result.EventId);
    }
    public static partial void ApplyRemoveEventViewDraw(IGameView view, ReadOnlySimObject sim, ApplyRemoveEventResult result)
    {
    }
    
    public static partial GDTask ApplyRemoveEventViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyRemoveEventResult result)
    {
        return GDTask.CompletedTask;
    }
}