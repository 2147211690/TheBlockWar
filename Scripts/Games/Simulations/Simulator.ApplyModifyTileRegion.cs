using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyTileRegion(Vector2I coords, string regionName, RegionOperation operation, 
            int duration, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new ModifyTileRegionStep(CurrentDeep, coords, regionName,
                        operation, duration, operation, duration));
                    return;
                }

                var tile = state.TileMapState.GetTile(coords);
                var resultOperation = RegionOperation.None;
                var resultDuration = duration;

                if (tile != TileState.Empty)
                {
                    (resultOperation, resultDuration) = operation switch
                    {
                        RegionOperation.AddRegion => state.HandleAddOrChangeRegion(tile, coords, regionName, duration,
                            true),
                        RegionOperation.RemoveRegion => state.HandleRemoveRegion(tile, coords, regionName),
                        RegionOperation.ChangeDuration => state.HandleAddOrChangeRegion(tile, coords, regionName,
                            duration * 2, false),
                        RegionOperation.SetDuration =>
                            state.HandleSetDurationRegion(tile, coords, regionName, duration),
                        _ => (RegionOperation.None, duration)
                    };
                }

                context.Steps?.AddStep(new ModifyTileRegionStep(CurrentDeep, coords, regionName, operation,
                    duration, resultOperation, resultDuration));

                if (ComplexSimulationRegion(context, 1))
                {
                    var unit = state.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull())
                    {
                        if (resultOperation == RegionOperation.AddRegion)
                        {
                            state.NotifyTileRegion(tile, regionName,
                                r => r.OnUnitEnter(state, coords, unit, coords, true, context));
                        }
                        else if (resultOperation == RegionOperation.RemoveRegion)
                        {
                            state.NotifyTileRegion(tile, regionName,
                                r => r.OnUnitLeave(state, coords, unit, coords, true, context));
                        }
                    }
                }
            }
        }

        private (RegionOperation operation, int duration) HandleAddOrChangeRegion(IReadOnlyTileState tile, Vector2I coords, 
            string regionName, int newDuration, bool isAdd)
        {
            if (tile.TryFindRegion(regionName, out var index))
            {
                var oldDuration = tile.Regions[index].Duration;
                var finalDuration = isAdd ? Mathf.Max(newDuration, oldDuration) : Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                    return (RegionOperation.RemoveRegion, 0);
                }
                
                state.TileMapState.ModifyTile(coords, t => t.ModifyRegion(index, finalDuration));
                return (RegionOperation.ChangeDuration, finalDuration - oldDuration);
            }
            
            state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionName, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }

        private (RegionOperation operation, int duration) HandleRemoveRegion(IReadOnlyTileState tile, Vector2I coords, 
            string regionName)
        {
            if (tile.TryFindRegion(regionName, out var index))
            {
                state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                return (RegionOperation.RemoveRegion, 0);
            }
            return (RegionOperation.None, 0);
        }

        private (RegionOperation operation, int duration) HandleSetDurationRegion(IReadOnlyTileState tile, Vector2I coords, 
            string regionName, int newDuration)
        {
            if (tile.TryFindRegion(regionName, out var index))
            {
                var oldDuration = tile.Regions[index].Duration;
                var finalDuration = Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                    return (RegionOperation.RemoveRegion, 0);
                }
                
                state.TileMapState.ModifyTile(coords, t => t.ModifyRegion(index, finalDuration));
                return (RegionOperation.ChangeDuration, finalDuration - oldDuration);
            }
            
            state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionName, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }
    }
}