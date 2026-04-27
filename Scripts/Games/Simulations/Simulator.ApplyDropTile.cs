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
                
                context.Steps?.AddStep(new DropTileStep(CurrentDeep, coords));
                if (!unit.IsNull())
                {
                    if (unit.IsWillDrop(state, coords))
                        state.ApplyDropUnit(coords, context);
                    else
                    {
                        state.ApplyUnitRemoveFromCoords(unit, coords, context);
                    }
                }
                state.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
            }
        }
    }
}