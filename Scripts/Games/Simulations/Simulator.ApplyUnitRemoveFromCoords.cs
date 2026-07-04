using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
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
                    foreach (var buff in region.RegionName.GetRegionInfo().RegionBuffs)
                        obj.ApplyModifyUnitBuff(unit.Coords, buff.BuffName, BuffOperation.AddBuff, buff.Count, false, true);
                }

                // 移除所有一次性区域
                foreach (var region in toTile.RegionsStatic)
                {
                    if (!region.RegionName.GetRegionInfo().IsOnece) continue;
                    obj.ApplyModifyTileRegion(coords, region.RegionName, RegionOperation.RemoveRegion, -1, string.Empty);
                }
            }
            ExitSimulation();
        }
    }
}