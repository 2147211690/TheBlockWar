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
                var r = obj.Steps.AddResult(new ApplyMoveUnitResult(CurrentDeep, CurrentTeam, coords, to, 0, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                var toUnit = obj.State.UnitMapState.GetUnit(to);
                r.ReUnitId = unit.Id;
                if (unit.IsNull() || !toUnit.IsNull()) return r;
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                r.ReValid = true;
                obj.ApplyUnitRemoveFromCoords(unit, coords);
                obj.ApplyUnitSetInCoords(unit);
                return r;
            }
        }
    }
    public static partial void ApplyMoveUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
        var uv = view.EventUnit(result.ReUnitId);
        if (uv == null || !result.ReValid) return;
        uv.EventFack(uv.WishCoords);
        uv.EventAimCoords = result.To;
    }
    public static partial GDTask ApplyMoveUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyMoveUnitResult result)
    {
        if (result.ReValid) view.SetUnitRealCoords(result.ReUnitId, result.To);
        return GDTask.CompletedTask;
    }
}