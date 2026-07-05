using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;

public class TestPhase : TurnPhase
{
    public override string PhaseName => "TestPhase";
    public override bool IsFocused { get; set; }
    public override GDTask Execute()
    {
        EventScheduler.CreateEvent(new StepsEventCore(
            new(3,3),
        [
            new ApplyDamageStep(new Vector2I(3, 3), 20),
        ]), Turn + 1, PhaseOrder, -1, SimulationLayer.Waiting);
        EventScheduler.RunAction((es, obj) =>
        {
            obj.ApplyModifyTileRegion(new Vector2I(0,0), 
                RegionNames.生成单位域, RegionOperation.AddRegion, 3, UnitNames.Unit0);
        });
        return GDTask.CompletedTask;
    }

    protected override void OnInit()
    {
    }
}