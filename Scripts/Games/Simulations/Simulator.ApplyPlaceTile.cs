using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        //TODO:tile改为枚举
        public void ApplyPlaceTile(Vector2I coords, string tileName, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new PlaceTileStep(CurrentDeep, coords, tileName));
                    return;
                }

                state.TileMapState.ModifyTile(coords, t =>
                {
                    t.Name = tileName;
                });
                context.Steps?.AddStep(new PlaceTileStep(CurrentDeep, coords, tileName));
                if (ComplexSimulationRegion(context, 2))
                {
                    var unit = state.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull()) state.ApplyUnitSetInCoords(unit, context);
                }
            }
        }
    }
}