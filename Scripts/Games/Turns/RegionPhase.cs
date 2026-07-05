using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

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
            field = value;
            _regionTurnEvent.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }
    private EventState _regionTurnEvent;
    public override async GDTask Execute()
    {
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
        ]), Turn, PhaseOrder, -1, SimulationLayer.Disabled, -100);
    }
}