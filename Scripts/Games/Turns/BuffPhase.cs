using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 用来处理buff的回合
/// </summary>
public class BuffPhase : ITurnPhase
{
    public string PhaseName { get; } = "BuffPhase";
    public int Turn { get; private set; }

    public bool IsFocused
    {
        get => field;
        set
        { 
            if (field == value) return;
            field = value;
            _buffTurnEvent?.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public ITurnManager TurnManager { get; private set; }
    private TurnEvent _buffTurnEvent;
    public ITurnPhase Init(ITurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        _buffTurnEvent = eventScheduler.CreateEvent(id => new TurnEvent(id)
        {
            Layer = SimulationLayer.Disabled,
            Turn = Turn,
            Priority = -1000,
            Team = -1,
            TurnType = TurnEvent.TypeEnum.Buff,
            PhaseOrder = PhaseOrder,
        });
        return this;
    }
    public async GDTask Execute()
    {
        _buffTurnEvent.Layer = SimulationLayer.Focused;
        await GDTask.Delay(800);
        _buffTurnEvent.Layer = SimulationLayer.Running;
        await EventScheduler.WaitCompleteRunning();
    }


}