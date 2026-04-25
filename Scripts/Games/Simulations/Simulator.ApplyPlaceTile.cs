using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyPlaceTile(Vector2I coords, TileState tile, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                    return;
                }

                state.TileMapState.ModifyTile(coords, t =>
                {
                    t.Name = tile.Name;
                    t.Tag = tile.Tag;
                });
                context.Steps?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                var unit = state.UnitMapState.GetUnit(coords);
                if (!unit.IsNull()) state.ApplyUnitSetInCoords(unit, context);
            }
        }
    }
}