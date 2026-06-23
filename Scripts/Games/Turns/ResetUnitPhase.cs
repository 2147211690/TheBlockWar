using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;

public class ResetUnitPhase : ITurnPhase
{
    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public GDTask Execute()
    {
        EventScheduler.RunAction(obj =>
        {
            foreach (var unit in obj.State.UnitMapState.GetAllUnits())
            {
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.CanUseSkill = CanUseSkillMask.All);
            }
        });
        return GDTask.CompletedTask;
    }

    public ITurnPhase Init(int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        return this;
    }
}