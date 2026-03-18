using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;
/// <summary>
/// 为了保证可预知性，模拟结果中的步骤必须按照顺序应用到游戏状态上
/// 任何对游戏的操作应该使用Event, 而不是直接修改游戏状态
/// Waiting层的模拟结果不应该主要用来标记,而不具体修改数据
/// </summary>
public static partial class Simulator
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
    }
}