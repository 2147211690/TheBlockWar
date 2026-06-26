using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 重置单位(技能使用次数)回合
/// </summary>
public class ResetUnitPhase : ITurnPhase
{
    public string PhaseName { get; } = "ResetUnitPhase";
    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public ITurnManager TurnManager { get; private set; }
    public GDTask Execute()
    {
        EventScheduler.RunAction(obj =>
        {
            obj.ApplyResetUnit();
        });
        return GDTask.CompletedTask;
    }

    public ITurnPhase Init(ITurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        return this;
    }
}