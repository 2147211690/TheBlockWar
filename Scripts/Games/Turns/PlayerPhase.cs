using TheBlockWar.Scripts.Games.Players;

namespace TheBlockWar.Scripts.Games.Turns;

public class PlayerPhase : ITurnPhase
{

    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public IPlayer Player { get; }
    public PlayerPhase(IPlayer player)
    {
        Player = player;
    }
    public ITurnPhase Init(int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        return this;
    }
    public async GDTask Execute()
    {
        Player.IsTurn = true;
        Player.PhaseOrder = PhaseOrder;
        await GDTask.WaitUntil(() => !Player.IsTurn);
    }


}