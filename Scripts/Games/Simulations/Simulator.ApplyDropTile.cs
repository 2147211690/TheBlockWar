using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyDropTile(Vector2I coords, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new DropTileStep(CurrentDeep, coords));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                state.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
                context.Steps?.AddStep(new DropTileStep(CurrentDeep, coords));
                if (!unit.IsNull())
                {
                    state.ApplyNextParallel(context);
                    state.ApplyDropUnit(coords, context);
                }
                var tile = state.TileMapState.GetTile(coords);
                /*for (var i = tile.Regions.Count - 1; tile.Regions.Count > 0; i = tile.Regions.Count - 1)
                {
                    state.ApplyModifyTileRegion(coords, tile.Regions[i].RegionType, RegionOperation.RemoveRegion, 0, result);
                }*/
                
            }
        }
    }
}