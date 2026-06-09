using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId", "Valid"], [typeof(long), typeof(bool)])]
        public void ApplyDropUnit(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyDropUnitResult(CurrentDeep, coords, 0, false));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                obj.State.UnitMapState.RemoveUnit(unit.Id);
                obj.Steps?.AddResult(new ApplyDropUnitResult(CurrentDeep, coords, unit.Id, true));
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