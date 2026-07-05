using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 重置单位(技能使用次数)回合
/// </summary>
public class ResetUnitPhase : TurnPhase
{
    public override string PhaseName { get; } = "ResetUnitPhase";
    public override bool IsFocused { get; set; }

    public override GDTask Execute()
    {
        EventScheduler.RunAction((es, obj) =>
        {
            obj.ApplyResetUnit();
        });
        return GDTask.CompletedTask;
    }

    protected override void OnInit()
    {
    }
}