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

    public static SimContext PreviousContext => _contexts.Count > 2 ? _contexts[^2] : new SimContext(0);
    public static SimContext CurrentContext => _contexts[^1];
    
    private static List<SimContext> _contexts = new([new SimContext(0)]);
    extension(SimObject obj)
    {
        public void PushContext(SimContext context)
        {
            var previous = CurrentContext;
            _contexts.Add(context);
            if (_contexts.Count == 2)
                obj.ApplyRaiseContextChanged(context);
            else 
                obj.TryRaiseContextChanged(context, previous);
        }

        public SimContext PopContext()
        {
            if (_contexts.Count == 1) return CurrentContext;
            var previous = _contexts[^1];
            _contexts.RemoveAt(_contexts.Count - 1);
            var current = _contexts[^1];
            obj.TryRaiseContextChanged(current, previous);
            return previous;
        }
        private void TryRaiseContextChanged(in SimContext current, in SimContext previous)
        {
            if (current != previous) obj.ApplyRaiseContextChanged(current);
        }
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
                    Hook.RegionUnitEnter(coords, obj, unit, coords, false);
                    foreach (var region in toTile.Regions)
                    foreach (var buff in region.RegionName.GetRegionInfo().RegionBuffs ?? [])
                        obj.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.AddBuff, 1, false, true);
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
                foreach (var region in fromTile.Regions)
                    foreach (var buff in region.RegionName.GetRegionInfo().RegionBuffs ?? [])
                        obj.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.RemoveBuff, 1, false, true);
                Hook.RegionUnitLeave(coords, obj, unit, coords, false);
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
            obj.Steps?.AddResult(new ApplyStepsResult(CurrentDeep, [], startDelay, endDelay, runningMode));
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