using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId1", "UnitId2", "Valid"], [typeof(long), typeof(long), typeof(bool)])]
        public ApplySwapUnitResult ApplySwapUnit(Vector2I coords1, Vector2I coords2)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplySwapUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplySwapUnitResult(
                    CurrentDeep, CurrentTeam, coords1, coords2, 0, 0, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit1 = obj.State.UnitMapState.GetUnit(coords1);
                var unit2 = obj.State.UnitMapState.GetUnit(coords2);
                r.ReUnitId1 = unit1.Id;
                r.ReUnitId2 = unit2.Id;
                
                if (!unit1.IsNull() && !unit2.IsNull())
                {
                    r.ReValid = true;
                    obj.State.UnitMapState.ModifyUnit(unit1.Id, u => u.Coords = coords2);
                    obj.State.UnitMapState.ModifyUnit(unit2.Id, u => u.Coords = coords1);
                    
                    obj.ApplyUnitRemoveFromCoords(unit1, coords1);
                    obj.ApplyUnitSetInCoords(unit1);
                    obj.ApplyUnitRemoveFromCoords(unit2, coords2);
                    obj.ApplyUnitSetInCoords(unit2);
                }
                else r.ReValid = false;
                return r;
            }
        }
    }

    public static partial void ApplySwapUnitReplayCore(IGameState state, ApplySwapUnitResult result)
    {
        if (!result.ReValid) return;
        state.UnitMapState.ModifyUnit(result.ReUnitId1, u => u.Coords = result.Coords2);
        state.UnitMapState.ModifyUnit(result.ReUnitId2, u => u.Coords = result.Coords1);
    }
    public static partial void ApplySwapUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplySwapUnitResult result)
    {
    }
    
    public static partial GDTask ApplySwapUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplySwapUnitResult result)
    {
        if (!result.ReValid) return GDTask.CompletedTask;
        var uv1 = view.GetUnitView(result.ReUnitId1);
        var uv2 = view.GetUnitView(result.ReUnitId2);
        uv1.EventFack(result.Coords1);
        uv2.EventFack(result.Coords2);
        view.SetUnitRealCoords(result.ReUnitId1, result.Coords2);
        view.SetUnitRealCoords(result.ReUnitId2, result.Coords1);
        return GDTask.CompletedTask;
    }
}