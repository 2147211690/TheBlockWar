namespace TheBlockWar.Scripts.Games.Turns.Phases;

public class UnitPlacePhase : TurnPhase
{
    public UnitPlacePhase(int team)
    {
        Team = team;
    }

    public override string PhaseName { get; } = "UnitPlacePhase";
    public override PhaseEnum PhaseType { get; } = PhaseEnum.PlaceUnit;
    public override int Team { get; }

    public override bool IsFocused
    {
        get => field;
        set
        {
            field = value;
            if (field)
            {
                EventScheduler.FocusedPlaceUnitTeamMask |= (1 << Team);
            }
            else
            {
                EventScheduler.FocusedPlaceUnitTeamMask &= ~(1 << Team);
            }
        }
    }

    public override void OnSetOrderTable(PhaseOrderTable orderTable)
    {
        orderTable.SetPlaceUnitOrder(Team, PhaseOrder);
    }
    public override async GDTask Execute()
    {
        EventScheduler.RunTeamUnitPlaceTurn(Team);
        await EventScheduler.WaitCompleteRunning();
    }

    protected override void OnInit()
    {
    }
}