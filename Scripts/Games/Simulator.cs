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
    extension(IGameState state)
    {
        public void ApplyResult(SimulationResult result)
        {
            foreach (var step in result.Steps)
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
                    case null:
                        break;
                }
            }

            return;
        }

        public void ApplyPlaceUnit(Vector2I coords, UnitState unitState,
            SimulationResult? result)
        {
            bool isValid = state.UnitMapState.GetUnit(coords) == null;
            if (result?.Layer == SimulationLayer.Waiting || !isValid)
            {
                result?.Steps.Add(new PlaceUnitStep(coords, null, unitState.Id, isValid));
                return;
            }
            unitState.Coords = coords;
            state.UnitMapState.AddUnit(unitState);
            result?.Steps.Add(new PlaceUnitStep(coords, unitState, unitState.Id, isValid));
        }

        public void ApplyDamage(Vector2I coords, float damage, SimulationResult? result)
        {
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new DamageStep(coords, damage, 0, 0));
                return;
            }

            var unit = state.UnitMapState.GetUnit(coords);
            float validDamage = 0;
            if (unit == null || unit.IsDead)
            {
                result?.Steps.Add(new DamageStep(coords, damage, unit?.Id ?? 0, validDamage));
                return;
            }

            var unitValue = unit;
            validDamage = Math.Min(damage, unitValue.Health);
            //GD.Print($"修改 {unit.Value.Id} 健康值 {unitValue.Health} - {validDamage}");
            state.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
            result?.Steps.Add(new DamageStep(coords, damage, unit.Id, validDamage));
            if (unitValue.Health <= 0)
            {
                ApplyDeath(state, coords, result);
            }
        }

        public void ApplyDeath(Vector2I coords, SimulationResult? result)
        {
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new DeathStep(coords, 0, false));
                
                return;
            }

            var unit = state.UnitMapState.GetUnit(coords);
            if (unit == null || unit.IsDead)
            {
                result?.Steps.Add(new DeathStep(coords, unit?.Id ?? 0, false));
                return;
            }

            result?.Steps.Add(new DeathStep(coords, unit.Id, true));
            state.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
        }

        public void ApplyDropTile(Vector2I coords, SimulationResult? result)
        {
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new DropTileStep(coords));
                return;
            }

            //TODO:测试上方单位等处理
            result?.Steps.Add(new DropTileStep(coords));
        }

        public void ApplyPlaceTile(Vector2I coords, TileState tile, SimulationResult? result)
        {
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new PlaceTileStep(coords, tile));
                return;
            }
            state.TileMapState.ModifyTile(coords, t =>
            {
                t.IsOccupied = tile.IsOccupied;
                t.Name = tile.Name;
                t.Tag = tile.Tag;
            });
            result?.Steps.Add(new PlaceTileStep(coords, tile));
        }

        public void ApplyMoveUnit(Vector2I coords, Vector2I to, SimulationResult? result)
        {
            var unit = state.UnitMapState.GetUnit(coords);
            var targetTile = state.TileMapState.GetTile(to);
            var targetUnit = state.UnitMapState.GetUnit(to);
            bool isValid = targetTile.Tag != TileTag.Empty && targetUnit == null && unit?.IsDead == false;
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new MoveUnitStep(coords, to, unit?.Id ?? 0, isValid));
                return;
            }

            if (isValid)
            {
                state.UnitMapState.ModifyUnit(unit!.Id, u => u.Coords = to);
                //TODO:引入坐标改变带来的其他影响
            }

            result?.Steps.Add(new MoveUnitStep(coords, to, unit?.Id ?? 0, isValid));
        }
        
        public void ApplyMoveUnitPath(Vector2I coords, List<Vector2I> path, SimulationResult? result)
        {
            if (path.Count == 0) return;
            var unit = state.UnitMapState.GetUnit(coords + path[0]);
            if (unit is null)
            {
                result?.Steps.Add(new MoveUnitPathStep(coords, path, 0, 0, false));
                return;
            }
            var targetIndex = 1;
            
            for (; targetIndex < path.Count - 1; targetIndex++)
                if (!unit.IsCanPass(state, coords + path[targetIndex])) break;
            
            for (; targetIndex >= 0; targetIndex--)
                if (unit.IsCanBeDestination(state, coords + path[targetIndex])) break;
            
            
            if (result?.Layer == SimulationLayer.Waiting)
            {
                result.Steps.Add(new MoveUnitPathStep(coords, path, unit.Id, targetIndex, true));
                return;
            }

            state.ApplyMoveUnit(coords + path[0], coords + path[targetIndex], null);

            result?.Steps.Add(new MoveUnitPathStep(coords, path, unit.Id, targetIndex, true));

        }
    }
}