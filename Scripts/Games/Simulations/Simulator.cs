using System.Linq;
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
    private static int _simulationStack = -1;
    private static int CurrentDeep => _simulationStack;
    private static void EnterSimulation()
    {
        if (_simulationStack == -1)
        {
            CurrentComplexSimulationMaxLevel = ComplexSimulationMaxLevel;
        }
        _simulationStack++;
    }
    private static void ExitSimulation() => _simulationStack--;
    private static int CurrentComplexSimulationMaxLevel { get; set; }
    private static bool ComplexSimulationRegion(SimulationContext context, int level) 
        => CurrentComplexSimulationMaxLevel >= level || context.Layer == SimulationLayer.Running;
    
    extension(IGameState state)
    {
        
        /// <summary>
        /// 触发单位进入到新坐标的事件
        /// </summary>
        private void ApplyUnitSetInCoords(IReadOnlyUnitState unit, SimulationContext context)
        {
            if (unit.IsNull())
            {
                return;
            }

            var coords = unit.Coords;
            if (unit.IsWillDrop(state, coords)) // 单位会掉落
            {
                state.ApplyDropUnit(coords, context);
            }
            else
            {
                var toTile = state.TileMapState.GetTile(coords);
                if (!toTile.IsNull())
                {
                    state.NotifyTileRegions(toTile, r => r.OnUnitEnter(state, coords, unit, coords, false, context));
                }
                // 移除所有一次性区域
                for (var i = toTile.Regions.Count - 1; i >= 0; i--)
                {
                    if (!toTile.Regions[i].RegionName.GetRegionInfo().IsOnece) continue;
                    state.TileMapState.ModifyTile(coords, u => u.RemoveRegion(i));
                }
            }
        }

        private void ApplyUnitRemoveFromCoords(IReadOnlyUnitState unit, Vector2I coords, SimulationContext context)
        {
            var fromTile = state.TileMapState.GetTile(coords);
            if (!fromTile.IsNull())
            {
                state.NotifyTileRegions(fromTile, r => r.OnUnitLeave(state, coords, unit, coords, false, context));
            }
        }

        private void NotifyUnitBuffs(IReadOnlyUnitState unit, Action<IBuff> action)
        {
            if (unit.Buffs.Count == 0) return;
            foreach (var buffType in unit.Buffs.ToArray())
            {
                action(buffType.BuffName.GetBuffInstance());
            }
        }

        private void NotifyUnitBuff(IReadOnlyUnitState unit, string buffName, Action<IBuff> action)
        {
            if (unit.IsNull()) return;
            if (!unit.TryFindBuff(buffName, out var index)) return;
            action(unit.Buffs[index].BuffName.GetBuffInstance());
        }

        private void NotifyTileRegions(IReadOnlyTileState tile, Action<IRegion> action)
        {
            if (tile.Regions.Count == 0) return;
            foreach (var regionType in tile.Regions.ToArray())
            {
                action(regionType.RegionName.GetRegionInstance());
            }
        }
        
        private void NotifyTileRegion(IReadOnlyTileState tile, string regionName, Action<IRegion> action)
        {
            if (tile == TileState.Empty) return;
            if (!tile.TryFindRegion(regionName, out var index)) return;
            action(tile.Regions[index].RegionName.GetRegionInstance());
        }
    }
}