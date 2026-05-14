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

    public static void PushContext(SimContext context) => _contexts.Push(context);
    public static SimContext PopContext() => _contexts.Pop();
    public static SimContext CurrentContext => _contexts.Peek();
    private static Stack<SimContext> _contexts = [];
    extension(SimObject obj)
    {
        private bool ComplexSimulationRegion(int level) 
            => CurrentComplexSimulationMaxLevel >= level || obj.Layer == SimulationLayer.Running;
        /// <summary>
        /// 触发单位进入到新坐标的事件
        /// </summary>
        private void ApplyUnitSetInCoords(IReadOnlyUnitState unit)
        {
            if (unit.IsNull())
            {
                return;
            }
            EnterSimulation();

            var coords = unit.Coords;
            if (unit.IsWillDrop(obj.State, coords)) // 单位会掉落
            {
                obj.ApplyDropUnit(coords);
            }
            else
            {
                var toTile = obj.State.TileMapState.GetTile(coords);
                if (!toTile.IsNull())
                {
                    obj.NotifyTileRegions(toTile, r => r.OnUnitEnter(obj, coords, unit, coords, false));
                    obj.NotifyTileRegions(toTile, r =>
                    {
                        var info = r.Name.GetRegionInfo();
                        if (info.RegionBuffs != null)
                        {
                            foreach (var buff in info.RegionBuffs)
                                obj.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.AddBuff, int.MaxValue);
                        }
                    });
                }
                // 移除所有一次性区域
                for (var i = toTile.Regions.Count - 1; i >= 0; i--)
                {
                    if (!toTile.Regions[i].RegionName.GetRegionInfo().IsOnece) continue;
                    obj.State.TileMapState.ModifyTile(coords, u => u.RemoveRegion(i));
                }
            }
            ExitSimulation();
        }

        private void ApplyUnitRemoveFromCoords(IReadOnlyUnitState unit, Vector2I coords)
        {
            EnterSimulation();
            var fromTile = obj.State.TileMapState.GetTile(coords);
            if (!fromTile.IsNull())
            {
                obj.NotifyTileRegions(fromTile, r => r.OnUnitLeave(obj, coords, unit, coords, false));
                obj.NotifyTileRegions(fromTile, r =>
                {
                    var info = r.Name.GetRegionInfo();
                    if (info.RegionBuffs != null)
                    {
                        foreach (var buff in info.RegionBuffs)
                            obj.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.RemoveBuff, int.MaxValue);
                    }
                });
            }
            ExitSimulation();
        }

        private void NotifyUnitBuffs(IReadOnlyUnitState unit, Action<IBuff> action)
        {
            if (unit.IsNull()) return;
            if (unit.Name.GetUnitInfo().Buffs is not {Length: > 0} && unit.Buffs.Count == 0) return;
            EnterSimulation();
            foreach (var name in unit.GetInfoAndCurrentBuffs(obj.State).ToArray())
            {
                action(name.GetBuffInstance());
            }
            ExitSimulation();
        }
        

        private void NotifyTileRegions(IReadOnlyTileState tile, Action<IRegion> action)
        {
            if (tile.IsNull()) return;
            if (tile.Name.GetTileInfo().Regions is not {Length: > 0} && tile.Regions.Count == 0) return;
            EnterSimulation();
            foreach (var name in tile.GetInfoAndCurrentRegions().ToArray())
            {
                action(name.GetRegionInstance());
            }
            ExitSimulation();
        }
        
        /// <summary>
        /// 开始封装一系列步骤, 请记住调用EndSteps结束
        /// </summary>
        public void BeginSteps(int startDelay, int endDelay, StepsRunningMode runningMode)
        {
            if (obj.Layer != SimulationLayer.Running) return;
            EnterSimulation();
            obj.Steps?.AddStep(new StepsStep(CurrentDeep, [], startDelay, endDelay, runningMode));
        }
        /// <summary>
        /// 结束封装一系列步骤
        /// </summary>
        public void EndSteps()
        {
            if (obj.Layer != SimulationLayer.Running) return;
            ExitSimulation();
        }
    }
}