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
        public void ApplySwapUnit(Vector2I coords1, Vector2I coords2)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplySwapUnitResult(CurrentDeep, coords1, coords2, 0, 0, false));
                    return;
                }

                var unit1 = obj.State.UnitMapState.GetUnit(coords1);
                var unit2 = obj.State.UnitMapState.GetUnit(coords2);
                
                if (!unit1.IsNull() && !unit2.IsNull())
                {
                    obj.State.UnitMapState.ModifyUnit(unit1.Id, u => u.Coords = coords2);
                    obj.State.UnitMapState.ModifyUnit(unit2.Id, u => u.Coords = coords1);
                    
                    obj.Steps?.AddResult(new ApplySwapUnitResult(CurrentDeep, coords1, coords2, unit1.Id, unit2.Id, true));
                    obj.ApplyUnitRemoveFromCoords(unit1, coords1);
                    obj.ApplyUnitSetInCoords(unit1);
                    obj.ApplyUnitRemoveFromCoords(unit2, coords2);
                    obj.ApplyUnitSetInCoords(unit2);
                }
                else obj.Steps?.AddResult(new ApplySwapUnitResult(CurrentDeep, coords1, coords2, unit1.Id, unit2.Id, false));
            }
        }
    }
    public static partial void ApplySwapUnitViewDraw(GameView view, ReadOnlySimObject sim, ApplySwapUnitResult result)
    {
    }
    
    public static partial GDTask ApplySwapUnitViewRun(GameView view, ReadOnlySimObject sim, ApplySwapUnitResult result)
    {
        if (!result.Valid) return GDTask.CompletedTask;
        view.UnitsView.SwapUnit(result.UnitId1, result.UnitId2);
        return GDTask.CompletedTask;
    }
}