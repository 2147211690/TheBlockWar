using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public void ApplyDropTile(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyDropTileResult(CurrentDeep, coords));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                
                obj.Steps?.AddResult(new ApplyDropTileResult(CurrentDeep, coords));
                obj.State.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
                if (!unit.IsNull())
                {
                    if (unit.IsWillDrop(obj.State, coords))
                        obj.ApplyDropUnit(coords);
                    else
                    {
                        obj.ApplyUnitRemoveFromCoords(unit, coords);
                    }
                }
            }
        }
    }
    public static partial void ApplyDropTileViewDraw(GameView view, ReadOnlySimObject sim, ApplyDropTileResult result)
    {
    }
    
    public static partial GDTask ApplyDropTileViewRun(GameView view, ReadOnlySimObject sim, ApplyDropTileResult result)
    {
        return view.TilesView.DropTile(result.Step.Coords);
    }
}