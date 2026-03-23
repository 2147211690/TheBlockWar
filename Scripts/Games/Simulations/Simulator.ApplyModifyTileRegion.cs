using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyTileRegion(Vector2I coords, RegionTypeEnum regionType, RegionOperation operation, 
            int duration, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyTileRegionStep(IsCurrentChildSimulation, coords, regionType,
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
                        RegionOperation.AddRegion => state.HandleAddOrChangeRegion(tile, coords, regionType, duration,
                            true),
                        RegionOperation.RemoveRegion => state.HandleRemoveRegion(tile, coords, regionType),
                        RegionOperation.ChangeDuration => state.HandleAddOrChangeRegion(tile, coords, regionType,
                            duration * 2, false),
                        RegionOperation.SetDuration =>
                            state.HandleSetDurationRegion(tile, coords, regionType, duration),
                        _ => (RegionOperation.None, duration)
                    };
                }

                result.Steps?.AddStep(new ModifyTileRegionStep(IsCurrentChildSimulation, coords, regionType, operation,
                    duration, resultOperation, resultDuration));

                if (ComplexSimulationRegion(result, 1))
                {
                    var unit = state.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull())
                    {
                        if (resultOperation == RegionOperation.AddRegion)
                        {
                            state.NotifyTileRegion(tile, regionType,
                                r => r.OnUnitEnter(state, coords, unit, coords, true, result));
                        }
                        else if (resultOperation == RegionOperation.RemoveRegion)
                        {
                            state.NotifyTileRegion(tile, regionType,
                                r => r.OnUnitLeave(state, coords, unit, coords, true, result));
                        }
                    }
                }
            }
        }

        private (RegionOperation operation, int duration) HandleAddOrChangeRegion(IReadOnlyTileState tile, Vector2I coords, 
            RegionTypeEnum regionType, int newDuration, bool isAdd)
        {
            if (tile.TryFindRegion(regionType, out var index))
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
            
            state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionType, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }

        private (RegionOperation operation, int duration) HandleRemoveRegion(IReadOnlyTileState tile, Vector2I coords, 
            RegionTypeEnum regionType)
        {
            if (tile.TryFindRegion(regionType, out var index))
            {
                state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                return (RegionOperation.RemoveRegion, 0);
            }
            return (RegionOperation.None, 0);
        }

        private (RegionOperation operation, int duration) HandleSetDurationRegion(IReadOnlyTileState tile, Vector2I coords, 
            RegionTypeEnum regionType, int newDuration)
        {
            if (tile.TryFindRegion(regionType, out var index))
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
            
            state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionType, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }
    }
}