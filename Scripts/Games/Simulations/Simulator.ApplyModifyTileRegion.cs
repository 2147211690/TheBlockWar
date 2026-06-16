using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step(["Operation", "OldDuration", "NewDuration", "Result"],
            [typeof(RegionOperation), typeof(int), typeof(int), typeof(RegionOperationResult)])]
        public ApplyModifyTileRegionResult ApplyModifyTileRegion(Vector2I coords, string regionName, RegionOperation operation,
            int duration, bool draw = true)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;
            
            ApplyModifyTileRegionResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyModifyTileRegionResult(CurrentDeep, coords, regionName,
                    operation, duration, draw, operation, 0, 0, RegionOperationResult.None));
                if (obj.Layer == SimulationLayer.Waiting) return r;

                var tile = obj.State.TileMapState.GetTile(coords);
                if (tile.IsNull()) return r;

                var index = tile.FindRegion(regionName);
                r.OldDuration = index == -1 ? 0 : tile.Regions[index].Duration;

                if (operation == RegionOperation.AddRegion)
                {
                    var newIndex = 0;
                    obj.State.TileMapState.ModifyTile(coords, t => newIndex = t.AddRegion(regionName, duration));
                    r.NewDuration = newIndex < 0 ? 0 : tile.Regions[newIndex].Duration;
                    if (r.OldDuration == 0 && r.NewDuration > 0) r.Result = RegionOperationResult.RegionAdded;
                    else if (r.NewDuration != r.OldDuration) r.Result = RegionOperationResult.RegionDurationChanged;
                }
                else if (operation == RegionOperation.RemoveRegion && index >= 0)
                {
                    var shouldRemove = true;
                    if (shouldRemove)
                    {
                        var newIndex = 0;
                        obj.State.TileMapState.ModifyTile(coords, t => newIndex = t.RemoveRegion(regionName, duration));
                        r.NewDuration = newIndex < 0 ? 0 : tile.Regions[newIndex].Duration;
                        if (r.NewDuration == 0 && r.OldDuration > 0) r.Result = RegionOperationResult.RegionRemoved;
                        else if (r.NewDuration != r.OldDuration) r.Result = RegionOperationResult.RegionDurationChanged;
                    }
                    else
                    {
                        r.Result = RegionOperationResult.None;
                        r.NewDuration = r.OldDuration;
                    }
                }

                if (obj.ComplexSimulationRegion(1))
                {
                    var unit = obj.State.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull())
                    {
                        switch (r.Result)
                        {
                            case RegionOperationResult.RegionAdded:
                                NotifyTileRegion(tile, regionName,
                                    reg => reg.OnUnitEnter(obj, coords, unit, coords, true));
                                break;
                            case RegionOperationResult.RegionRemoved:
                                NotifyTileRegion(tile, regionName,
                                    reg => reg.OnUnitLeave(obj, coords, unit, coords, true));
                                break;
                        }
                    }
                }

                return r;
            }

            void NotifyTileRegion(IReadOnlyTileState tile, string regionName, Action<IRegion> action)
            {
                if (tile.IsNull()) return;
                if (tile.FindRegion(regionName) == -1) return;
                EnterSimulation();
                obj.PushContext(new SimContext(0));
                action(regionName.GetRegionInfo().Hook);
                obj.PopContext();
                ExitSimulation();
            }
        }
    }
    public static partial void ApplyModifyTileRegionViewDraw(GameView view, ReadOnlySimObject sim, ApplyModifyTileRegionResult result)
    {
    }
    
    public static partial GDTask ApplyModifyTileRegionViewRun(GameView view, ReadOnlySimObject sim, ApplyModifyTileRegionResult result)
    {
        return GDTask.CompletedTask;
    }
}