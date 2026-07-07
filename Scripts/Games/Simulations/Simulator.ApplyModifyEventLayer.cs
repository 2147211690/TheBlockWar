using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyModifyEventLayerResult ApplyModifyEventLayer(long eventId, SimulationLayer layer)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyModifyEventLayerResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyModifyEventLayerResult(
                    CurrentDeep, CurrentTeam, eventId, layer));
                if (obj.Layer == SimulationLayer.Waiting) return r;
                if (obj.State.EventManagerState.Events.TryGetValue(eventId, out var eventCore))
                {
                    eventCore.Layer = layer;
                }
                return r;
            }
        }
    }
    public static partial void ApplyModifyEventLayerReplayCore(IGameState state, ApplyModifyEventLayerResult result)
    {
        if (state.EventManagerState.Events.TryGetValue(result.EventId, out var eventCore))
        {
            eventCore.Layer = result.Layer;
        }
    }
    public static partial void ApplyModifyEventLayerViewDraw(IGameView view, ReadOnlySimObject sim, ApplyModifyEventLayerResult result)
    {
    }
    
    public static partial GDTask ApplyModifyEventLayerViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyModifyEventLayerResult result)
    {
        return GDTask.CompletedTask;
    }
}