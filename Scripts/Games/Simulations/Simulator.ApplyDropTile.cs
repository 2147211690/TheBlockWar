using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyDropTile(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new DropTileStep(CurrentDeep, coords));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                
                obj.Steps?.AddStep(new DropTileStep(CurrentDeep, coords));
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
}