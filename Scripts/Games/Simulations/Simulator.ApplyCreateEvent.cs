using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step(["Event"], [typeof(EventState)])]
        public ApplyCreateEventResult ApplyCreateEvent(EventCore core,
            int turn, int phaseOrder, int eventTeam,
            SimulationLayer layer,
            int priority = 0)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyCreateEventResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyCreateEventResult(
                    CurrentDeep, CurrentTeam, core, turn, phaseOrder, eventTeam, layer, priority));
                if (obj.Layer == SimulationLayer.Waiting) return r;
                r.ReEvent = obj.State.CreateEvent(core, turn, phaseOrder, eventTeam, layer, priority).Clone();
                return r;
            }
        }
    }
    public static partial void ApplyCreateEventReplayCore(IGameState state, ApplyCreateEventResult result)
    {
        state.EventManagerState.AddEvent(result.ReEvent);
    }
    public static partial void ApplyCreateEventViewDraw(IGameView view, ReadOnlySimObject sim, ApplyCreateEventResult result)
    {
    }
    
    public static partial GDTask ApplyCreateEventViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyCreateEventResult result)
    {
        return GDTask.CompletedTask;
    }
}