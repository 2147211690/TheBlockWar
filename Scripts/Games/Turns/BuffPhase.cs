using TheBlockWar.Scripts.Games.Events;
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
            _buffTurnEvent?.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }

    private TurnEvent _buffTurnEvent = null!;
    protected override void OnInit()
    {
        _buffTurnEvent = EventScheduler.CreateEvent(id => new TurnEvent(id)
        {
            Layer = SimulationLayer.Disabled,
            Turn = Turn,
            Priority = -1000,
            Team = -1,
            TurnType = TurnEvent.TypeEnum.Buff,
            PhaseOrder = PhaseOrder,
        });
    }
    public override async GDTask Execute()
    {
        _buffTurnEvent.Layer = SimulationLayer.Focused;
        await GDTask.Delay(800);
        _buffTurnEvent.Layer = SimulationLayer.Running;
        await EventScheduler.WaitCompleteRunning();
    }
}