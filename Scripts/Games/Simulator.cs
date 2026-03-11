using System;
using Godot;

namespace TheBlockWar.Scripts.Games;
/// <summary>
/// 为了保证可预知性，模拟结果中的步骤必须按照顺序应用到游戏状态上
/// 任何对游戏的操作应该使用Event, 而不是直接修改游戏状态
/// Waiting层的模拟结果不应该主要用来标记,而不具体修改数据
/// </summary>
public static class Simulator
{
    public static void ApplyResult(IGameState state, SimulationResult result)
    {
        foreach (var step in result.Steps)
        {
            switch (step)
            {
                case DamageStep damageStep:
                    GD.Print($"伤害 {damageStep.Coords} {damageStep.Damage} {damageStep.ResultValidDamage}");
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
                case null:
                    break;
            }
        }
        return;
    }

    public static void ApplyPlaceUnit(IGameState state, Vector2I coords, in UnitState unitState ,SimulationResult? result)
    {
        bool isValid = state.GetUnit(coords) == null;
        if (result?.Layer == SimulationLayer.Waiting || !isValid)
        {
            result?.Steps.Add(new PlaceUnitStep(coords, UnitState.Default, unitState.Id, isValid));
            return;
        }
        state.AddUnit(unitState with { Coords = coords });
        result?.Steps.Add(new PlaceUnitStep(coords, unitState, unitState.Id, isValid));
    }
    public static void ApplyDamage(IGameState state, Vector2I coords, float damage, SimulationResult? result)
    {
        if (result?.Layer == SimulationLayer.Waiting)
        {
            result.Steps.Add(new DamageStep(coords, damage, 0, 0));
            return;
        }
        var unit = state.GetUnit(coords);
        float validDamage = 0;
        if (unit == null || unit.Value.IsDead)
        {
            result?.Steps.Add(new DamageStep(coords, damage, unit?.Id ?? 0, validDamage));
            return;
        }
        var unitValue = unit.Value;
        validDamage = Math.Min(damage, unitValue.Health);
        unitValue.Health -= validDamage;
        GD.Print($"修改 {unit.Value.Id} 健康值 {unitValue.Health} - {validDamage}");
        state.ModifyUnit(unit.Value.Id,  u => unitValue);
        result?.Steps.Add(new DamageStep(coords, damage, unit.Value.Id, validDamage));
        if (unitValue.Health <= 0)
        {
            ApplyDeath(state, coords, result);
        }
    }

    public static void ApplyDeath(IGameState state, Vector2I coords, SimulationResult? result)
    {
        if (result?.Layer == SimulationLayer.Waiting)
        {
            result.Steps.Add(new DeathStep(coords, 0, false));
            return;
        }
        var unit = state.GetUnit(coords);
        if (unit == null || unit.Value.IsDead)
        {
            result?.Steps.Add(new DeathStep(coords, unit?.Id ?? 0, false));
            return;
        }
        result?.Steps.Add(new DeathStep(coords, unit.Value.Id, true));
        state.ModifyUnit(unit.Value.Id, u => u with { IsDead = true });
    }
    public static void DropTile(IGameState state, Vector2I coords, SimulationResult? result)
    {
        if (result?.Layer == SimulationLayer.Waiting)
        {
            result.Steps.Add(new DropTileStep(coords));
            return;
        }
        state.MapState.SetTile(coords, TileState.Empty);
        //TODO:测试上方单位等处理
        result?.Steps.Add(new DropTileStep(coords));
    }

    public static void ApplyPlaceTile(IGameState state, Vector2I coords, in TileState tile, SimulationResult? result)
    {
        if (result?.Layer == SimulationLayer.Waiting)
        {
            result.Steps.Add(new PlaceTileStep(coords, tile));
            return;
        }
        state.MapState.SetTile(coords, tile);
        result?.Steps.Add(new PlaceTileStep(coords, tile));
    }

    public static void ApplyMoveUnit(IGameState state, Vector2I coords, Vector2I to, SimulationResult? result)
    {
        var unit = state.GetUnit(coords);
        var targetTile = state.MapState.GetTile(to);
        var targetUnit = state.GetUnit(to);
        //bool isValid = targetTile.Tag != TileTag.Empty && targetUnit == null && unit?.IsDead == false;
        bool isValid = true;
        if (result?.Layer == SimulationLayer.Waiting)
        {
            result.Steps.Add(new MoveUnitStep(coords, to, unit?.Id ?? 0, isValid));
            return;
        }
        if (isValid)
        {
            state.ModifyUnit(unit!.Value.Id, u => u with { Coords = to });
            //TODO:引入坐标改变带来的其他影响
        }
        result?.Steps.Add(new MoveUnitStep(coords, to, unit?.Id ?? 0, isValid));
    }
    
}