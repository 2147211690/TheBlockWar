using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Turns.Phases;

public class TestPhase : TurnPhase
{
    public override string PhaseName => "TestPhase";
    public override PhaseEnum PhaseType { get; } = PhaseEnum.Test;
    public override int Team { get; } = 0;
    public override bool IsFocused { get; set; }
    public override GDTask Execute()
    {
        EventScheduler.SetTeamPreparePlaceUnit(1, 0, new PreparePlaceUnitInfo(new(5,5), UnitNames.Unit1));
        return GDTask.CompletedTask;
    }

    protected override void OnInit()
    {
    }
}