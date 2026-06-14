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
        public ApplyMoveUnitResult ApplyMoveUnit(Vector2I coords, Vector2I to)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyMoveUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyMoveUnitResult(CurrentDeep, coords, to, 0, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                var toUnit = obj.State.UnitMapState.GetUnit(to);
                r.UnitId = unit.Id;
                if (unit.IsNull() || !toUnit.IsNull()) return r;
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                r.Valid = true;
                obj.ApplyUnitRemoveFromCoords(unit, coords);
                obj.ApplyUnitSetInCoords(unit);
                return r;
            }
        }
    }
    public static partial void ApplyMoveUnitViewDraw(GameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
        var uv = view.UnitsView.GetUnitView(result.UnitId);
        if (uv != null && result.Valid)
        {
            uv.AddFack(uv.WishCoords);
            view.UnitsView.SetUnitAimCoords(result.UnitId, result.Step.To);
            view.EventsView.SetUnitNeedClear(result.UnitId);
        }
    }
    
    public static partial GDTask ApplyMoveUnitViewRun(GameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
        if (result.Valid) view.UnitsView.MoveUnit(result.UnitId, result.Step.To);
        view.UnitsView.UpdateUnitPosition(result.Step.To);
        return GDTask.CompletedTask;
    }
}