using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:Operation:RegionOperation Duration:int
        public void ApplyModifyTileRegion(Vector2I coords, string regionName, RegionOperation operation, 
            int duration)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new ModifyTileRegionStep(CurrentDeep, coords, regionName,
                        operation, duration, operation, duration));
                    return;
                }

                var tile = obj.State.TileMapState.GetTile(coords);
                var oldDuration = tile.GetRegionDuration(regionName);
                var resultOperation = RegionOperation.None;
                var resultDuration = duration;

                if (!tile.IsNull())
                {
                    (resultOperation, resultDuration) = operation switch
                    {
                        RegionOperation.AddRegion => obj.HandleAddOrChangeRegion(tile, coords, regionName, duration,
                            true),
                        RegionOperation.RemoveRegion => obj.HandleRemoveRegion(tile, coords, regionName),
                        RegionOperation.ChangeDuration => obj.HandleAddOrChangeRegion(tile, coords, regionName,
                            oldDuration + duration, false),
                        RegionOperation.SetDuration =>
                            obj.HandleSetDurationRegion(tile, coords, regionName, duration),
                        _ => (RegionOperation.None, duration)
                    };
                }

                obj.Steps?.AddStep(new ModifyTileRegionStep(CurrentDeep, coords, regionName, operation,
                    duration, resultOperation, resultDuration));

                if (obj.ComplexSimulationRegion(1))
                {
                    var unit = obj.State.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull())
                    {
                        if (resultOperation == RegionOperation.AddRegion)
                        {
                            NotifyTileRegion(tile, regionName,
                                r => r.OnUnitEnter(obj, coords, unit, coords, true));
                        }
                        else if (resultOperation == RegionOperation.RemoveRegion)
                        {
                            NotifyTileRegion(tile, regionName,
                                r => r.OnUnitLeave(obj, coords, unit, coords, true));
                        }
                    }
                }
            }
            void NotifyTileRegion(IReadOnlyTileState tile, string regionName, Action<IRegion> action)
            {
                if (tile.IsNull()) return;
                if (!tile.TryFindRegion(regionName, out var index)) return;
                EnterSimulation();
                action(tile.Regions[index].RegionName.GetRegionInfo().Hook);
                ExitSimulation();
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
                    obj.State.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                    return (RegionOperation.RemoveRegion, 0);
                }
                
                obj.State.TileMapState.ModifyTile(coords, t => t.ModifyRegion(index, finalDuration));
                return (RegionOperation.ChangeDuration, finalDuration - oldDuration);
            }
            
            obj.State.TileMapState.ModifyTile(coords, t => t.AddRegion(regionName, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }

        private (RegionOperation operation, int duration) HandleRemoveRegion(IReadOnlyTileState tile, Vector2I coords, 
            string regionName)
        {
            if (tile.TryFindRegion(regionName, out var index))
            {
                obj.State.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
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
                    obj.State.TileMapState.ModifyTile(coords, t => t.RemoveRegion(index));
                    return (RegionOperation.RemoveRegion, 0);
                }
                
                obj.State.TileMapState.ModifyTile(coords, t => t.ModifyRegion(index, finalDuration));
                return (RegionOperation.ChangeDuration, finalDuration - oldDuration);
            }
            
            obj.State.TileMapState.ModifyTile(coords, t => t.AddRegion(regionName, newDuration));
            return (RegionOperation.AddRegion, newDuration);
        }
    }
}