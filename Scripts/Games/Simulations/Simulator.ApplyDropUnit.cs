using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId"], [typeof(long)])]
        public ApplyDropUnitResult ApplyDropUnit(Vector2I coords)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyDropUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyDropUnitResult(CurrentDeep, CurrentTeam, coords, 0));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.ReUnitId = unit.Id;
                obj.State.UnitMapState.RemoveUnit(unit.Id);
                return r;
            }
        }
    }
    public static partial void ApplyDropUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyDropUnitResult result)
    {
    }
    
    public static partial async GDTask ApplyDropUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyDropUnitResult result)
    {
        var uv = view.GetUnitView(result.ReUnitId);
        if (uv == null) return;
        await AnimationHelper.DropToAsync(uv, uv.GamePosition + new Vector3(0, 0, -80), 0.24f);
        view.FreeUnitView(result.ReUnitId);
    }
}