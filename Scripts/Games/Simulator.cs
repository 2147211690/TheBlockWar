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
    private static int _simulationStack = 0;
    private static bool IsCurrentMasterSimulation => _simulationStack == 1;
    private static bool IsCurrentChildSimulation => _simulationStack > 1;
    private static void EnterSimulation() => _simulationStack++;
    private static void ExitSimulation() => _simulationStack--;

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
                    case null:
                        break;
                }
            }

            return;
        }
        /// <summary>
        /// 触发单位进入到新坐标的事件
        /// </summary>
        private void ApplyUnitSetInCoords(IReadOnlyUnitState? unit, SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            if (unit is null)
            {
                renewedUnit = null;
                return;
            }
            
            var coords = unit.Coords;
            if (unit.IsWillDrop(state, coords)) // 单位会掉落
            {
                state.ApplyDropUnit(coords, result);
            }
            
            renewedUnit = state.UnitMapState.GetUnit(unit.Id);
        }
        private void ApplyUnitRemoveFromCoords(IReadOnlyUnitState? unit, in Vector2I coords, SimulationResult result, out IReadOnlyUnitState? renewedUnit)
        {
            if (unit is null)
            {
                renewedUnit = null;
                return;
            }
            
            renewedUnit = state.UnitMapState.GetUnit(unit.Id);
        }

        private void NotifyUnitBuffs(IReadOnlyUnitState? unit, Action<IBuff> action)
        {
            if ((unit?.Buffs.Count ?? 0) == 0)
            {
                return;
            }
            foreach (var buffType in unit.Buffs.ToArray())
            {
                if (unit is null) break;
                action(buffType.BuffType.GetInstance());
            }
        }
        private void NotifyUnitBuff(IReadOnlyUnitState? unit, BuffTypeEnum buffType, Action<IBuff> action)
        {
            if (unit is null) return;
            if (!unit.TryFindBuff(buffType, out var index)) return;
            action(unit.Buffs[index].BuffType.GetInstance());
        }
        public void ApplyPlaceUnit(Vector2I coords, UnitState unitState,
            SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = state.UnitMapState.GetUnit(coords) is null;
                if (result.Layer == SimulationLayer.Waiting || !isValid)
                {
                    result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, null, unitState.Id, isValid));
                    return;
                }
                unitState.Coords = coords;
                state.UnitMapState.AddUnit(unitState, out var unit);
                result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitState, unitState.Id, isValid));
                state.ApplyUnitSetInCoords(unit!, result, out _);
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
                var id = unit?.Id ?? 0;
                if (unit is null)
                {
                    result.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }
                state.UnitMapState.RemoveUnit(id, out unit);
                state.ApplyUnitRemoveFromCoords(unit, coords, result, out _);
                result.Steps?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, id, true));
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
                var id = unit?.Id ?? 0;
                float validDamage = 0;
                if (unit is null || unit.IsDead)
                {
                    result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, id, validDamage));
                    return;
                }
                var rawDamage = damage;
                
                state.NotifyUnitBuffs(unit, b => b.OnUnitBeforeDamage(state, unit, rawDamage, ref rawDamage, result, out unit));

                if (unit is null)
                {
                    result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, 0, validDamage));
                    return;
                }
                
                validDamage = Math.Min(rawDamage, unit.Health);
                state.UnitMapState.ModifyUnit(id, u => u.Health -= validDamage, out unit);
                
                if (unit is null)
                {
                    result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, 0, validDamage));
                    return;
                }
                
                result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, id, validDamage));

                // 触发受伤后事件, 仅在运行层触发
                //TODO:待考虑是否需要在关注层触发
                if (validDamage > 0 && result.Layer == SimulationLayer.Running)
                {
                    // 触发受伤后事件
                    state.NotifyUnitBuffs(unit, b => b.OnUnitAfterDamage(state, unit, validDamage, result, out unit));
                }
                
                if (unit?.Health <= 0)
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
                var id = unit?.Id ?? 0;
                
                if (unit is null || unit.IsDead)
                {
                    result.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, id, false));
                    return;
                }
                
                result.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, id, true));
                state.UnitMapState.ModifyUnit(id, u => u.IsDead = true, out unit);
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
                state.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t), out _);
                result.Steps?.AddStep(new DropTileStep(IsCurrentChildSimulation, coords));
                if (unit != null) state.ApplyDropUnit(coords, result);
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
                }, out _);
                result.Steps?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                var unit = state.UnitMapState.GetUnit(coords);
                if (unit is not null) state.ApplyUnitSetInCoords(unit, result, out unit);
            }
        }

        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                var unit = state.UnitMapState.GetUnit(coords);
                var id = unit?.Id ?? 0;
                bool isValid = true;
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, id, isValid));
                    return;
                }

                if (unit is not null)
                {
                    state.ApplyUnitRemoveFromCoords(unit, coords, result, out unit);
                    state.UnitMapState.ModifyUnit(id, u => u.Coords = to, out unit);
                }
                result.Steps?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, id, isValid));
                if (unit is not null) state.ApplyUnitSetInCoords(unit, result, out unit);
            }
        }
        
        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I> path, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (path.Count == 0) return;
                var unit = state.UnitMapState.GetUnit(coords + path[0]);
                var id = unit?.Id ?? 0;
                if (unit is null)
                {
                    result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, id, 0, false));
                    return;
                }
                var targetIndex = 1;
            
                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(state, coords + path[targetIndex])) break;
            
                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(state, coords + path[targetIndex])) break;

            
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, id, targetIndex, true));
                    return;
                }
                
                result.Steps?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, id, targetIndex, true));
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
                var id = unit?.Id ?? 0;
                var target = coords + dir.ToVec2I();
                var targetUnit = state.UnitMapState.GetUnit(target);
                var validPush = targetUnit is null && state.IsInMap(target);
                result.Steps?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, id, validPush));
                if (unit is null) return;
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
                out BuffOperation resultOperation, out int resultDuration, out IReadOnlyUnitState? renewedUnit)
            {
                if (newDuration == 0)
                {
                    //如果新持续时间为0，则移除buff
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(changeIndex), out renewedUnit);
                    resultOperation = BuffOperation.RemoveBuff;
                    resultDuration = 0;
                }
                else
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(changeIndex, newDuration), out renewedUnit);
                    var change = newDuration - oldDuration;
                    resultOperation = BuffOperation.ChangeDuration;
                    resultDuration = change;
                }
            }

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation, duration,
                        0, operation, duration));
                    return;
                }
                var unit = state.UnitMapState.GetUnit(coords);
                var id = unit?.Id ?? 0;
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;
                if (unit is not null)
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
                                    out resultOperation, out resultDuration, out unit);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(id, u => u.AddBuff(buffType, duration), out unit);
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }
                            break;
                        case BuffOperation.RemoveBuff: //移除buff
                            if (unit.TryFindBuff(buffType, out var removeIndex))
                            {
                                //如果buff已存在
                                state.UnitMapState.ModifyUnit(id, u => u.RemoveBuff(removeIndex), out unit);
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
                                    out resultOperation, out resultDuration, out unit);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(id, u => u.AddBuff(buffType, duration), out unit);
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
                                    out resultOperation, out resultDuration, out unit);
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(id, u => u.AddBuff(buffType, duration), out unit);
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }
                            break;
                    }
                }

                result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation, duration,
                    id, resultOperation, resultDuration));
                
                switch (resultOperation)
                {
                    case BuffOperation.AddBuff:
                        state.NotifyUnitBuffs(unit, b => b.OnAddToUnit(state, unit, result, out unit));
                        break;
                    case BuffOperation.RemoveBuff:
                        state.NotifyUnitBuff(unit, buffType, b => b.OnRemoveFromUnit(state, unit, result, out unit));
                        break;
                    case BuffOperation.ChangeDuration:
                        state.NotifyUnitBuff(unit, buffType, b => b.OnDurationChanged(state, unit,
                            resultOldDuration, resultOldDuration + resultDuration, result, out unit));
                        break;
                    default:
                        break;
                }
            }
        }
    }
}