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
                var r = obj.Steps.AddResult(new ApplyDropUnitResult(CurrentDeep, coords, 0));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.UnitId = unit.Id;
                obj.State.UnitMapState.RemoveUnit(unit.Id);
                return r;
            }
        }
    }
    public static partial void ApplyDropUnitViewDraw(GameView view, ReadOnlySimObject sim, ApplyDropUnitResult result)
    {
    }
    
    public static partial GDTask ApplyDropUnitViewRun(GameView view, ReadOnlySimObject sim, ApplyDropUnitResult result)
    {
        return view.UnitsView.DropUnit(result.UnitId);
    }
}