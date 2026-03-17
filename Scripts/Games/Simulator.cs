using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games;
/// <summary>
/// 为了保证可预知性，模拟结果中的步骤必须按照顺序应用到游戏状态上
/// 任何对游戏的操作应该使用Event, 而不是直接修改游戏状态
/// Waiting层的模拟结果不应该主要用来标记,而不具体修改数据
/// </summary>
public static class Simulator
{
    /// <summary>
    /// 是否进行复杂模拟
    /// </summary>
    public static int ComplexSimulationMaxLevel { get; set; } = int.MaxValue;
    private static int _simulationStack;
    private static bool IsCurrentChildSimulation => _simulationStack > 1;
    private static void EnterSimulation()
    {
        if (_simulationStack == 0)
        {
            CurrentComplexSimulationMaxLevel = ComplexSimulationMaxLevel;
        }
        _simulationStack++;
    }
    private static void ExitSimulation() => _simulationStack--;
    private static int CurrentComplexSimulationMaxLevel { get; set; }
    private static bool ComplexSimulationRegion(SimulationResult result, int level) 
        => CurrentComplexSimulationMaxLevel >= level || result.Layer == SimulationLayer.Running;
    
    extension(IGameState state)
    {
        public void ApplyResult(SimulationResult result)
        {
            //运行站位
            var runningResult = new SimulationResult(0, result.TeamType, SimulationLayer.Running, false);
            foreach (var step in result.MasterSteps)
            {
                switch (step)
                {
                    case DamageStep damageStep:
                        //GD.Print($"伤害 {damageStep.Coords} {damageStep.Damage} {damageStep.ResultValidDamage}");
                        ApplyDamage(state, damageStep.Coords, damageStep.Damage, runningResult);
                        break;
                    case DeathStep deathStep:
                        ApplyDeath(state, deathStep.Coords, runningResult);
                        break;
                    case PlaceTileStep placeTileStep:
                        ApplyPlaceTile(state, placeTileStep.Coords, placeTileStep.Tile, runningResult);
                        break;
                    case PlaceUnitStep placeUnitStep:
                        ApplyPlaceUnit(state, placeUnitStep.Coords, placeUnitStep.Unit, runningResult);
                        break;
                    case MoveUnitStep moveStep:
                        ApplyMoveUnit(state, moveStep.Coords, moveStep.To, runningResult);
                        break;
                    case MoveUnitPathStep movePathStep:
                        ApplyMoveUnitPath(state, movePathStep.Coords, movePathStep.Path, runningResult);
                        break;
                    case DropUnitStep dropStep:
                        ApplyDropUnit(state, dropStep.Coords, runningResult);
                        break;
                    case PushUnitStep pushStep:
                        ApplyPushUnit(state, pushStep.Coords, pushStep.PushDir, runningResult);
                        break;
                    case DropTileStep dropTileStep:
                        ApplyDropTile(state, dropTileStep.Coords, runningResult);
                        break;
                    case ModifyUnitBuffStep modifyUnitBuffStep:
                        ApplyModifyUnitBuff(state, modifyUnitBuffStep.Coords, modifyUnitBuffStep.BuffType,
                            modifyUnitBuffStep.Operation, modifyUnitBuffStep.Duration, runningResult);
                        break;
                    case ModifyTileRegionStep modifyTileRegionStep:
                        ApplyModifyTileRegion(state, modifyTileRegionStep.Coords, modifyTileRegionStep.RegionType,
                            modifyTileRegionStep.Operation, modifyTileRegionStep.Duration, runningResult);
                        break;
                    case null:
                        break;
                }
            }
        }
        /// <summary>
        /// 触发单位进入到新坐标的事件
        /// </summary>
        private void ApplyUnitSetInCoords(IReadOnlyUnitState unit, SimulationResult result)
        {
            if (unit.IsNull())
            {
                return;
            }

            var coords = unit.Coords;
            if (unit.IsWillDrop(state, coords)) // 单位会掉落
            {
                state.ApplyDropUnit(coords, result);
            }
            else
            {
                var toTile = state.TileMapState.GetTile(unit.Coords);
                if (!toTile.IsNull())
                {
                    state.NotifyTileRegions(toTile, r => r.OnUnitEnter(state, unit, coords, false, result));
                }
            }
        }

        private void ApplyUnitRemoveFromCoords(IReadOnlyUnitState unit, Vector2I coords, SimulationResult result)
        {
            var fromTile = state.TileMapState.GetTile(coords);
            if (!fromTile.IsNull())
            {
                state.NotifyTileRegions(fromTile, r => r.OnUnitLeave(state, unit, coords, false, result));
            }
        }

        private void NotifyUnitBuffs(IReadOnlyUnitState unit, Action<IBuff> action)
        {
            if (unit.Buffs.Count == 0) return;
            foreach (var buffType in unit.Buffs.ToArray())
            {
                action(buffType.BuffType.GetInstance());
            }
        }

        private void NotifyUnitBuff(IReadOnlyUnitState unit, BuffTypeEnum buffType, Action<IBuff> action)
        {
            if (unit.IsNull()) return;
            if (!unit.TryFindBuff(buffType, out var index)) return;
            action(unit.Buffs[index].BuffType.GetInstance());
        }

        private void NotifyTileRegions(IReadOnlyTileState tile, Action<IRegion> action)
        {
            if (tile.Regions.Count == 0) return;
            foreach (var regionType in tile.Regions.ToArray())
            {
                action(regionType.RegionType.GetInstance());
            }
        }

        private void NotifyTileRegion(IReadOnlyTileState tile, RegionTypeEnum regionType, Action<IRegion> action)
        {
            if (tile == TileState.Empty) return;
            if (!tile.TryFindRegion(regionType, out var index)) return;
            action(tile.Regions[index].RegionType.GetInstance());
        }

        public void ApplyPlaceUnit(Vector2I coords, UnitState unitState,
            SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = state.UnitMapState.GetUnit(coords).IsNull();
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, null, 0, isValid));
                    return;
                }

                //如果在等待层,或者此处有单位,则放置失败
                if (!isValid)
                {
                    result.Steps?.AddStep(
                        new PlaceUnitStep(IsCurrentChildSimulation, coords, null, unitState.Id, false));
                    return;
                }

                unitState.Coords = coords;
                state.UnitMapState.AddUnit(unitState);
                result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitState, unitState.Id,
                    isValid));
                //应用单位放置到坐标的额外操作
                state.ApplyUnitSetInCoords(state.UnitMapState.GetUnit(unitState.Id), result);
            }
        }

        public void ApplyDropUnit(Vector2I coords, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                state.UnitMapState.RemoveUnit(unit.Id);
                result.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, unit.Id, true));
            }
        }

        public void ApplyDamage(Vector2I coords, float damage, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, 0, 0));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                float validDamage = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                var rawDamage = damage;

                //应用单位伤害前事件
                if (ComplexSimulationRegion(result, 0))
                {
                    state.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeDamage(state, unit, rawDamage, ref rawDamage, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                validDamage = Math.Min(rawDamage, unit.Health);
                state.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                unit = state.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));

                // 触发受伤后事件, 仅在运行层触发
                if (validDamage > 0 && ComplexSimulationRegion(result, 0))
                {
                    // 触发受伤后事件
                    state.NotifyUnitBuffs(unit, b => b.OnUnitAfterDamage(state, unit, validDamage, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (!unit.IsNull() && unit.Health <= 0)
                {
                    ApplyDeath(state, coords, result);
                }
            }
        }

        public void ApplyDeath(Vector2I coords, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);

                if (unit.IsNull() || unit.IsDead)
                {
                    result.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit.Id, false));
                    return;
                }

                result.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit.Id, true));
                state.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
            }
        }

        public void ApplyDropTile(Vector2I coords, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new DropTileStep(IsCurrentChildSimulation, coords));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                state.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
                result.Steps?.AddStep(new DropTileStep(IsCurrentChildSimulation, coords));
                if (!unit.IsNull()) state.ApplyDropUnit(coords, result);
                var tile = state.TileMapState.GetTile(coords);
                /*for (var i = tile.Regions.Count - 1; tile.Regions.Count > 0; i = tile.Regions.Count - 1)
                {
                    state.ApplyModifyTileRegion(coords, tile.Regions[i].RegionType, RegionOperation.RemoveRegion, 0, result);
                }*/
                
            }
        }

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

        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                bool isValid = true;
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, 0, isValid));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                if (!unit.IsNull())
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                    result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit.Id, isValid));
                    state.ApplyUnitRemoveFromCoords(unit, coords, result);
                    state.ApplyUnitSetInCoords(unit, result);
                }
                else result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit.Id, isValid));
            }
        }

        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I>? path, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (path is not {Count: > 0}) return;
                var unit = state.UnitMapState.GetUnit(coords + path[0]);
                if (unit.IsNull())
                {
                    result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, 0,
                        false));
                    return;
                }

                var targetIndex = 1;

                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(state, coords + path[targetIndex]))
                        break;

                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(state, coords + path[targetIndex]))
                        break;


                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id,
                        targetIndex, true));
                    return;
                }

                result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, targetIndex,
                    true));
                if (targetIndex > 0) state.ApplyMoveUnit(coords + path[0], coords + path[targetIndex], result);
            }
        }

        public void ApplyPushUnit(Vector2I coords, DirMask dir, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, 0, true));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                var target = coords + dir.ToVec2I();
                var targetUnit = state.UnitMapState.GetUnit(target);
                var validPush = !unit.IsNull() && targetUnit.IsNull() && state.IsInMap(target);
                result.Steps?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, unit.Id, validPush));
                if (unit.IsNull()) return;
                if (validPush)
                {
                    state.ApplyMoveUnit(coords, target, result);
                }
                else
                {
                    state.ApplyDamage(coords, 10, result);
                    state.ApplyDamage(target, 10, result);
                }
            }
        }

        public void ApplyModifyUnitBuff(Vector2I coords, BuffTypeEnum buffType, BuffOperation operation, int duration,
            SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();


            void ChangeUnitValidBuff(IReadOnlyUnitState unit, int changeIndex, int oldDuration, int newDuration,
                out BuffOperation resultOperation, out int resultDuration)
            {
                if (newDuration == 0)
                {
                    //如果新持续时间为0，则移除buff
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(changeIndex));
                    resultOperation = BuffOperation.RemoveBuff;
                    resultDuration = 0;
                }
                else
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(changeIndex, newDuration));
                    var change = newDuration - oldDuration;
                    resultOperation = BuffOperation.ChangeDuration;
                    resultDuration = change;
                }
            }

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
                        duration,
                        0, operation, duration));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;
                if (!unit.IsNull())
                {
                    switch (operation)
                    {
                        case BuffOperation.AddBuff: //添加buff
                            if (unit.TryFindBuff(buffType, out var addIndex))
                            {
                                //如果buff已存在，则使用最大持续时间
                                var oldDuration = unit.Buffs[addIndex].Duration;
                                var newDuration = Mathf.Max(duration, oldDuration);
                                ChangeUnitValidBuff(unit, addIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                        case BuffOperation.RemoveBuff: //移除buff
                            if (unit.TryFindBuff(buffType, out var removeIndex))
                            {
                                //如果buff已存在
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(removeIndex));
                                resultOperation = BuffOperation.RemoveBuff;
                                resultDuration = 0;
                            }

                            break;
                        case BuffOperation.ChangeDuration: //改变buff持续时间
                            if (unit.TryFindBuff(buffType, out var changeIndex))
                            {
                                var oldDuration = unit.Buffs[changeIndex].Duration;
                                var newDuration = Mathf.Max(duration + duration, 0);
                                //如果buff已存在
                                ChangeUnitValidBuff(unit, changeIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                        case BuffOperation.SetDuration: //设置buff持续时间
                            if (unit.TryFindBuff(buffType, out var setIndex))
                            {
                                //如果buff已存在
                                var oldDuration = unit.Buffs[setIndex].Duration;
                                var newDuration = Mathf.Max(duration, 0);
                                ChangeUnitValidBuff(unit, setIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                    }
                }

                result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
                    duration,
                    unit.Id, resultOperation, resultDuration));

                if (ComplexSimulationRegion(result, 1))
                {
                    switch (resultOperation)
                    {
                        case BuffOperation.AddBuff:
                            state.NotifyUnitBuffs(unit, b => b.OnAddToUnit(state, unit, result));
                            break;
                        case BuffOperation.RemoveBuff:
                            state.NotifyUnitBuff(unit, buffType, b => b.OnRemoveFromUnit(state, unit, result));
                            break;
                        case BuffOperation.ChangeDuration:
                            state.NotifyUnitBuff(unit, buffType, b => b.OnDurationChanged(state, unit,
                                resultOldDuration, resultOldDuration + resultDuration, result));
                            break;
                        default:
                            break;
                    }
                }
            }
        }

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

        public static void ParallelStart(SimulationResult result)
        {
            if (result.Layer != SimulationLayer.Running) return;
            result.Steps?.AddStep(new ParallelStep(IsCurrentChildSimulation, true));
        }

        public static void ParallelEnd(SimulationResult result)
        {
            if (result.Layer != SimulationLayer.Running) return;
            result.Steps?.AddStep(new ParallelStep(IsCurrentChildSimulation, false));
        }
    }
}