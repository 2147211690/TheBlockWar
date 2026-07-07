using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 区域回合
/// </summary>
public class RegionPhase : TurnPhase
{
    public override string PhaseName { get; } = "RegionPhase";
    public override bool IsFocused
    {
        get => field;
        set
        { 
            if (field == value) return;
            _prevRegionEvents?.SetLayer(SimulationLayer.Waiting);
            field = value;
            _prevRegionEvents = field ? GetRegionEvents() : null;
            _prevRegionEvents?.SetLayer(SimulationLayer.Focused);
            _regionTurnEvent.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }
    private List<EventState>? _prevRegionEvents;
    private EventState _regionTurnEvent;
    public override async GDTask Execute()
    {
        foreach (var se in GetRegionEvents())
        {
            if (!se.CheckIsValid(EventScheduler.GameState)) continue;
            se.Layer = SimulationLayer.Focused;
            await GDTask.Delay(800);
            se.Layer = SimulationLayer.Disabled;
            se.Layer = SimulationLayer.Running;
            await EventScheduler.WaitCompleteRunning();
            await GDTask.Delay(100);
        }
        
        _regionTurnEvent.Layer = SimulationLayer.Focused;
        await GDTask.Delay(800);
        _regionTurnEvent.Layer = SimulationLayer.Running;
        await EventScheduler.WaitCompleteRunning();
    }
    protected override void OnInit()
    {

        _regionTurnEvent = EventScheduler.CreateEvent(new StepsEventCore(
            null,
            [
                new ApplyRegionTurnStep(Turn)
            ]), PhaseOrder, -1, SimulationLayer.Disabled, 100, EventAfterRunMode.Remove);
    }

    private List<EventState> GetRegionEvents()
    {
        return EventScheduler.Events.Values.Where(e => e.PhaseOrder == PhaseOrder).ToList();
    }
}