using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId", "Valid"], [typeof(long), typeof(bool)])]
        public void ApplyMoveUnit(Vector2I coords, Vector2I to)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyMoveUnitResult(CurrentDeep, coords, to, 0, false));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                if (!unit.IsNull())
                {
                    obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                    obj.Steps?.AddResult(new ApplyMoveUnitResult(CurrentDeep, coords, to, unit.Id, true));
                    obj.ApplyUnitRemoveFromCoords(unit, coords);
                    obj.ApplyUnitSetInCoords(unit);
                }
                else obj.Steps?.AddResult(new ApplyMoveUnitResult(CurrentDeep, coords, to, unit.Id, false));
            }
        }
    }
    public static partial void ApplyMoveUnitViewDraw(GameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
    }
    
    public static partial GDTask ApplyMoveUnitViewRun(GameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
        if (result.Valid) view.UnitsView.MoveUnit(result.UnitId, result.Step.To);
        view.UnitsView.UpdateUnitPosition(result.Step.To);
        return GDTask.CompletedTask;
    }
}