using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyTileRegion(Vector2I coords, RegionTypeEnum regionType, RegionOperation operation,
            int duration,
            SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void ChangeTileValidRegion(IReadOnlyTileState tile, int changeIndex, int oldDuration, int newDuration,
                out RegionOperation resultOperation, out int resultDuration)
            {
                if (newDuration == 0)
                {
                    state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(changeIndex));
                    resultOperation = RegionOperation.RemoveRegion;
                    resultDuration = 0;
                }
                else
                {
                    state.TileMapState.ModifyTile(coords, t => t.ModifyRegion(changeIndex, newDuration));
                    var change = newDuration - oldDuration;
                    resultOperation = RegionOperation.ChangeDuration;
                    resultDuration = change;
                }
            }

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyTileRegionStep(IsCurrentChildSimulation, coords, regionType,
                        operation,
                        duration,
                        operation, duration));
                    return;
                }

                var tile = state.TileMapState.GetTile(coords);
                var resultOperation = RegionOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;
                if (tile != TileState.Empty)
                {
                    switch (operation)
                    {
                        case RegionOperation.AddRegion:
                            if (tile.TryFindRegion(regionType, out var addIndex))
                            {
                                var oldDuration = tile.Regions[addIndex].Duration;
                                var newDuration = Mathf.Max(duration, oldDuration);
                                ChangeTileValidRegion(tile, addIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionType, duration));
                                resultOperation = RegionOperation.AddRegion;
                                resultDuration = duration;
                            }

                            break;
                        case RegionOperation.RemoveRegion:
                            if (tile.TryFindRegion(regionType, out var removeIndex))
                            {
                                state.TileMapState.ModifyTile(coords, t => t.RemoveRegion(removeIndex));
                                resultOperation = RegionOperation.RemoveRegion;
                                resultDuration = 0;
                            }

                            break;
                        case RegionOperation.ChangeDuration:
                            if (tile.TryFindRegion(regionType, out var changeIndex))
                            {
                                var oldDuration = tile.Regions[changeIndex].Duration;
                                var newDuration = Mathf.Max(duration + duration, 0);
                                ChangeTileValidRegion(tile, changeIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionType, duration));
                                resultOperation = RegionOperation.AddRegion;
                                resultDuration = duration;
                            }

                            break;
                        case RegionOperation.SetDuration:
                            if (tile.TryFindRegion(regionType, out var setIndex))
                            {
                                var oldDuration = tile.Regions[setIndex].Duration;
                                var newDuration = Mathf.Max(duration, 0);
                                ChangeTileValidRegion(tile, setIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                            }
                            else
                            {
                                state.TileMapState.ModifyTile(coords, t => t.AddRegion(regionType, duration));
                                resultOperation = RegionOperation.AddRegion;
                                resultDuration = duration;
                            }

                            break;
                    }
                }

                result.Steps?.AddStep(new ModifyTileRegionStep(IsCurrentChildSimulation, coords, regionType, operation,
                    duration,
                    resultOperation, resultDuration));

                if (ComplexSimulationRegion(result, 1))
                {
                    switch (resultOperation)
                    {
                        case RegionOperation.AddRegion:

                            var unit = state.UnitMapState.GetUnit(coords);
                            if (!unit.IsNull())
                            {
                                state.NotifyTileRegion(tile, regionType,
                                    r => r.OnUnitEnter(state, unit, coords, true, result));
                            }

                            break;
                        case RegionOperation.RemoveRegion:
                            var removeUnit = state.UnitMapState.GetUnit(coords);
                            if (!removeUnit.IsNull())
                            {
                                state.NotifyTileRegion(tile, regionType,
                                    r => r.OnUnitLeave(state, removeUnit, coords, true, result));
                            }

                            break;
                        case RegionOperation.ChangeDuration:
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}