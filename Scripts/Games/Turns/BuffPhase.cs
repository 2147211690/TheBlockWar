using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 用来处理buff的回合
/// </summary>
public class BuffPhase : TurnPhase
{
    public override string PhaseName { get; } = "BuffPhase";

    public override bool IsFocused
    {
        get => field;
        set
        { 
            if (field == value) return;
            field = value;
            _buffTurnEvent.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }
    private EventState _buffTurnEvent = null!;
    protected override void OnInit()
    {
        _buffTurnEvent = EventScheduler.CreateEvent(new StepsEventCore(
            null,
            [
                new ApplyBuffTurnStep(Turn)
            ]
        ), PhaseOrder, -1, SimulationLayer.Disabled, -100, EventAfterRunMode.Remove);
    }
    public override async GDTask Execute()
    {
        _buffTurnEvent.Layer = SimulationLayer.Focused;
        await GDTask.Delay(800);
        _buffTurnEvent.Layer = SimulationLayer.Running;
        await EventScheduler.WaitCompleteRunning();
    }
}