using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step(["OldDuration", "NewDuration", "Result", "EventId"],
            [typeof(int), typeof(int), typeof(RegionOperationResult), typeof(long)])]
        public ApplyModifyTileRegionResult ApplyModifyTileRegion(Vector2I coords, string regionName, RegionOperation operation,
            int duration, string? regionData, bool draw = true)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;
            
            ApplyModifyTileRegionResult Main()
            {
                var r = obj.Steps.AddResult(
                    new ApplyModifyTileRegionResult(CurrentDeep, CurrentTeam, coords, regionName,
                    operation, duration, regionData, draw, 
                    0, 0, RegionOperationResult.None));
                if (obj.Layer == SimulationLayer.Waiting) return r;

                var tile = obj.State.TileMapState.GetTile(coords);
                if (tile.IsNull()) return r;

                var index = tile.FindRegion(regionName);
                r.ReOldDuration = index == -1 ? 0 : tile.Regions[index].Duration;
                var oldEventId = index == -1 ? 0 : tile.Regions[index].RegionEventId;
                
                if (operation == RegionOperation.ChangeRegionData)
                {
                    obj.State.TileMapState.ModifyTile(coords, 
                        t => t.ChangeRegionData(regionName, regionData));
                    r.ReResult = RegionOperationResult.RegionDataChanged;
                    r.ReNewDuration = r.ReOldDuration;
                }
                else if (operation == RegionOperation.AddRegion)
                {
                    obj.State.TileMapState.ModifyTile(coords, t =>
                    {
                        index = t.AddRegion(regionName, duration, regionData);
                    });
                    r.ReNewDuration = index < 0 ? 0 : tile.Regions[index].Duration;
                    if (r.ReOldDuration == 0 && r.ReNewDuration > r.ReOldDuration) r.ReResult = RegionOperationResult.RegionAdded;
                    else if (r.ReNewDuration != r.ReOldDuration) r.ReResult = RegionOperationResult.RegionDurationChanged;
                }
                else if (operation == RegionOperation.RemoveRegion && index >= 0)
                {
                    var shouldRemove = true;
                    if (shouldRemove)
                    {
                        obj.State.TileMapState.ModifyTile(coords, t => index = t.RemoveRegion(regionName, duration));
                        r.ReNewDuration = index < 0 ? 0 : tile.Regions[index].Duration;
                        if (r.ReNewDuration == 0 && r.ReNewDuration < r.ReOldDuration) r.ReResult = RegionOperationResult.RegionRemoved;
                        else if (r.ReNewDuration != r.ReOldDuration) r.ReResult = RegionOperationResult.RegionDurationChanged;
                    }
                    else
                    {
                        r.ReResult = RegionOperationResult.None;
                        r.ReNewDuration = r.ReOldDuration;
                    }
                }

                if (obj.ComplexSimulationRegion(1))
                {
                    var unit = obj.State.UnitMapState.GetUnit(coords);
                    switch (r.ReResult)
                    {
                        case RegionOperationResult.RegionAdded:
                            if (!unit.IsNull())
                            {
                                NotifyTileRegion(tile, regionName,
                                    reg => reg.OnUnitEnter(obj, coords, regionData, unit, coords, true));
                            }
                            Hook.RegionAddToTile(coords, obj);
                            
                            IEventCore? core = null;
                            regionName.GetRegionInfo().Hook.OnCreateRegionEvent(obj, coords, regionData, ref core);
                            if (core != null)
                            {
                                var er = obj.ApplyCreateEvent(core,
                                    obj.State.TurnState.GetPhaseOrder(obj.State.TurnState.CurrentTurn + 1, "RegionPhase"),
                                    -1, SimulationLayer.Waiting, 0, EventAfterRunMode.ToWaiting);
                                obj.State.TileMapState.ModifyTile(coords, t => t.ChangeRegionEventId(index, er.ReEvent.Id));
                                r.ReEventId = er.ReEvent.Id;
                            }
                            break;
                        case RegionOperationResult.RegionRemoved:
                            if (!unit.IsNull())
                            {
                                NotifyTileRegion(tile, regionName,
                                    reg => reg.OnUnitLeave(obj, coords, tile.Regions[index].RegionData ,unit, coords, true));
                            }
                            Hook.RegionRemoveFromTile(coords, obj);

                            if (oldEventId != 0)
                            {
                                obj.ApplyRemoveEvent(oldEventId);
                            }
                            break;
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
    public static partial void ApplyModifyTileRegionReplayCore(IGameState state, ApplyModifyTileRegionResult result)
    {
        state.TileMapState.ModifyTile(result.Coords, t =>
        {
            switch (result.ReResult)
            {
                case RegionOperationResult.None:
                    break;
                case RegionOperationResult.RegionAdded or RegionOperationResult.RegionRemoved or RegionOperationResult.RegionDurationChanged:
                    var change = result.ReNewDuration - result.ReOldDuration;
                    if (change > 0)
                    {
                        var index = t.AddRegion(result.RegionName, change);
                        t.ChangeRegionData(index, result.RegionData);
                    }
                    else if (change < 0)
                    {
                        t.RemoveRegion(result.RegionName, -change);
                    }

                    if (result.ReResult == RegionOperationResult.RegionAdded)
                    {
                        t.ChangeRegionEventId(result.RegionName, result.ReEventId);
                    }
                    break;
                case RegionOperationResult.RegionDataChanged:
                    t.ChangeRegionData(result.RegionName, result.RegionData);
                    break;
            }
        });
    }
    public static partial void ApplyModifyTileRegionViewDraw(IGameView view, ReadOnlySimObject sim, ApplyModifyTileRegionResult result)
    {
    }
    
    public static partial GDTask ApplyModifyTileRegionViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyModifyTileRegionResult result)
    {
        return GDTask.CompletedTask;
    }
}