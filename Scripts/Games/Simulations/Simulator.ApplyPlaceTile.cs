using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyPlaceTile(Vector2I coords, TileState tile, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                    return;
                }

                state.TileMapState.ModifyTile(coords, t =>
                {
                    t.IsOccupied = tile.IsOccupied;
                    t.Name = tile.Name;
                    t.Tag = tile.Tag;
                });
                result.Steps?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                var unit = state.UnitMapState.GetUnit(coords);
                if (!unit.IsNull()) state.ApplyUnitSetInCoords(unit, result);
            }
        }
    }
}