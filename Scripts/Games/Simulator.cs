using System;
using System.Collections.Generic;
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
            foreach (var step in result.MasterSteps)
            {
                switch (step)
                {
                    case DamageStep damageStep:
                        //GD.Print($"伤害 {damageStep.Coords} {damageStep.Damage} {damageStep.ResultValidDamage}");
                        ApplyDamage(state, damageStep.Coords, damageStep.Damage, null);
                        break;
                    case DeathStep deathStep:
                        ApplyDeath(state, deathStep.Coords, null);
                        break;
                    case PlaceTileStep placeTileStep:
                        ApplyPlaceTile(state, placeTileStep.Coords, placeTileStep.Tile, null);
                        break;
                    case PlaceUnitStep placeUnitStep:
                        ApplyPlaceUnit(state, placeUnitStep.Coords, placeUnitStep.Unit, null);
                        break;
                    case MoveUnitStep moveStep:
                        ApplyMoveUnit(state, moveStep.Coords, moveStep.To, null);
                        break;
                    case MoveUnitPathStep movePathStep:
                        ApplyMoveUnitPath(state, movePathStep.Coords, movePathStep.Path, null);
                        break;
                    case DropUnitStep dropStep:
                        ApplyDropUnit(state, dropStep.Coords, null);
                        break;
                    case PushUnitStep pushStep:
                        ApplyPushUnit(state, pushStep.Coords, pushStep.PushDir, null);
                        break;
                    case DropTileStep dropTileStep:
                        ApplyDropTile(state, dropTileStep.Coords, null);
                        break;
                    case ModifyUnitBuffStep modifyUnitBuffStep:
                        ApplyModifyUnitBuff(state, modifyUnitBuffStep.Coords, modifyUnitBuffStep.BuffType, 
                            modifyUnitBuffStep.Operation, modifyUnitBuffStep.Duration, null);
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
        /// <param name="unit"></param>
        private void ApplyUnitSetInCoords(uint unitId, SimulationResult? result)
        {
            var unit = state.UnitMapState.GetUnit(unitId); //防止获取到镜像中未改变的单位
            if (unit is null) return;
            var coords = unit.Coords;
            if (unit.IsWillDrop(state, coords)) // 单位会掉落
            {
                state.ApplyDropUnit(coords, result);
            }
        }
        private void ApplyUnitRemoveFromCoords(uint unitId, in Vector2I coords, SimulationResult? result)
        {
            
        }
        public void ApplyPlaceUnit(Vector2I coords, UnitState unitState,
            SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = state.UnitMapState.GetUnit(coords) is null;
                if (result?.Layer == SimulationLayer.Waiting || !isValid)
                {
                    result?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, null, unitState.Id, isValid));
                    return;
                }
                unitState.Coords = coords;
                state.UnitMapState.AddUnit(unitState);
                result?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitState, unitState.Id, isValid));
                state.ApplyUnitSetInCoords(unitState.Id, result);
            }
        }
        public void ApplyDropUnit(Vector2I coords, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }
                var unit = state.UnitMapState.GetUnit(coords);
                if (unit is null)
                {
                    result?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }
                state.UnitMapState.RemoveUnit(unit.Id);
                state.ApplyUnitRemoveFromCoords(unit.Id, coords, result);
                result?.AddStep(new DropUnitStep(IsCurrentChildSimulation, coords, unit.Id, true));
            }
        }
        
        public void ApplyDamage(Vector2I coords, float damage, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, 0, 0));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                float validDamage = 0;
                if (unit is null || unit.IsDead)
                {
                    result?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, unit?.Id ?? 0, validDamage));
                    return;
                }

                var unitValue = unit;
                validDamage = Math.Min(damage, unitValue.Health);
                //GD.Print($"修改 {unit.Value.Id} 健康值 {unitValue.Health} - {validDamage}");
                state.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                result?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                if (unitValue.Health <= 0)
                {
                    ApplyDeath(state, coords, result);
                }
            }
        }

        public void ApplyDeath(Vector2I coords, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new DeathStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }
                
                var unit = state.UnitMapState.GetUnit(coords);
                if (unit is null || unit.IsDead)
                {
                    result?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit?.Id ?? 0, false));
                    return;
                }
                
                result?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit.Id, true));
                state.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
            }
        }

        public void ApplyDropTile(Vector2I coords, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new DropTileStep(IsCurrentChildSimulation, coords));
                    return;
                }
                var unit = state.UnitMapState.GetUnit(coords);
                state.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
                result?.AddStep(new DropTileStep(IsCurrentChildSimulation, coords));
                if (unit != null) state.ApplyDropUnit(coords, result);
            }
        }

        public void ApplyPlaceTile(Vector2I coords, TileState tile, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                    return;
                }
                state.TileMapState.ModifyTile(coords, t =>
                {
                    t.IsOccupied = tile.IsOccupied;
                    t.Name = tile.Name;
                    t.Tag = tile.Tag;
                });
                result?.AddStep(new PlaceTileStep(IsCurrentChildSimulation, coords, tile));
                var unit = state.UnitMapState.GetUnit(coords);
                if (unit != null) state.ApplyUnitSetInCoords(unit.Id, result);
            }
        }

        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                var unit = state.UnitMapState.GetUnit(coords);
                bool isValid = true;
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit?.Id ?? 0, isValid));
                    return;
                }

                if (unit is not null)
                {
                    state.ApplyUnitRemoveFromCoords(unit.Id, coords, result);
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                }
                result?.AddStep(new MoveUnitStep(IsCurrentChildSimulation, coords, to, unit?.Id ?? 0, isValid));
                if (unit is not null) state.ApplyUnitSetInCoords(unit.Id, result);
            }
        }
        
        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I> path, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (path.Count == 0) return;
                var unit = state.UnitMapState.GetUnit(coords + path[0]);
                if (unit is null)
                {
                    result?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, 0, 0, false));
                    return;
                }
                var targetIndex = 1;
            
                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(state, coords + path[targetIndex])) break;
            
                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(state, coords + path[targetIndex])) break;

            
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, targetIndex, true));
                    if (targetIndex > 0) state.ApplyMoveUnit(coords + path[0], coords + path[targetIndex], result);
                    return;
                }
            
                result?.AddStep(new MoveUnitPathStep(IsCurrentChildSimulation, coords, path, unit.Id, targetIndex, true));
                if (targetIndex > 0) state.ApplyMoveUnit(coords + path[0], coords + path[targetIndex], result);
            }
        }

        public void ApplyPushUnit(Vector2I coords, DirMask dir, SimulationResult? result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, 0, true));
                    return;
                }
                var unit = state.UnitMapState.GetUnit(coords);
                var target = coords + dir.ToVec2I();
                var targetUnit = state.UnitMapState.GetUnit(target);
                var validPush = targetUnit is null && state.IsInMap(target);
                result?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, unit?.Id ?? 0, validPush));
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
            SimulationResult? result)
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
                if (result?.Layer == SimulationLayer.Waiting)
                {
                    result.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation, duration,
                        0, operation, duration));
                    return;
                }
                var unit = state.UnitMapState.GetUnit(coords);
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
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
                    unit = state.UnitMapState.GetUnit(unit.Id)!;
                    //TODO:处理buff改变后的额外效果
                }

                result?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation, duration,
                    unit?.Id ?? 0, resultOperation, resultDuration));
            }
        }
    }
}