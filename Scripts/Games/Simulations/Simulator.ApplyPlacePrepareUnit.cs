using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitState:UnitState Valid:bool
        [Step(["UnitState", "Valid"], [typeof(UnitState), typeof(bool)])]
        public ApplyPlacePrepareUnitResult ApplyPlacePrepareUnit(int prepareTeam, int index)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyPlacePrepareUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyPlacePrepareUnitResult(
                    CurrentDeep, CurrentTeam, prepareTeam, index, UnitState.Null, false));
                var place = obj.State.TeamState.GetTeamPreparePlaceUnit(prepareTeam, index);
                r.ReValid = place is not null;
                if (!r.ReValid)
                {
                    return r;
                }
                var placeUnit = obj.ApplyPlaceUnit(place!.Value.Coords, place.Value.UnitName, prepareTeam);
                if (r.ReValid)
                {
                    
                    r.ReUnitState = placeUnit.ReUnitState;
                    obj.State.TeamState.SetTeamPreparePlaceUnit(prepareTeam, index, null);
                }
                return r;
            }
        }
    }
    public static partial void ApplyPlacePrepareUnitReplayCore(IGameState state, ApplyPlacePrepareUnitResult result)
    {
        if (!result.ReValid) return;
        state.TeamState.SetTeamPreparePlaceUnit(result.PrepareTeam, result.Index, null);
    }
    public static partial void ApplyPlacePrepareUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyPlacePrepareUnitResult result)
    {
    }
    
    public static partial GDTask ApplyPlacePrepareUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyPlacePrepareUnitResult result)
    {
        return GDTask.CompletedTask;
    }
}