using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Turns;

public class RegionPhase : ITurnPhase
{
    public int Turn { get; private set; }

    public bool IsFocused
    {
        get => field;
        set
        { 
            if (field == value) return;
            field = value;
            _regionTurnEvent?.Layer = field ? SimulationLayer.Focused : SimulationLayer.Disabled;
        }
    }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public TurnManager TurnManager { get; private set; }
    private TurnEvent _regionTurnEvent;
    public async GDTask Execute()
    {
        _regionTurnEvent.Layer = SimulationLayer.Focused;
        await GDTask.Delay(800);
        _regionTurnEvent.Layer = SimulationLayer.Running;
        await EventScheduler.WaitCompleteRunning();
    }

    public ITurnPhase Init(TurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        _regionTurnEvent = eventScheduler.CreateEvent(id => new TurnEvent(id)
        {
            Layer = SimulationLayer.Disabled,
            Turn = Turn,
            Priority = -1000,
            Team = -1,
            TurnType = TurnEvent.TypeEnum.Region,
            PhaseOrder = PhaseOrder,
        });
        return this;
    }
}