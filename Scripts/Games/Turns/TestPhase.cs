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
        EventScheduler.CreateEvent(id => new StepsEvent(id)
        {
            Steps =
            [
                new ApplyDamageStep(new Vector2I(3,3), 20),
            ],
            Layer = SimulationLayer.Waiting,
            PhaseOrder = PhaseOrder,
            Turn = Turn + 1,
            Coords = new Vector2I(3, 3)
        });
        EventScheduler.RunAction(obj =>
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