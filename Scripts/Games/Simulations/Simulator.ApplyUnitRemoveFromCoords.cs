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
        /// 触发单位从某个坐标移除后的事件
        /// </summary>
        private void ApplyUnitRemoveFromCoords(IReadOnlyUnitState unit, Vector2I coords)
        {
            EnterSimulation();
            var fromTile = obj.State.TileMapState.GetTile(coords);
            if (!fromTile.IsNull())
            {
                foreach (var region in fromTile.Regions)
                foreach (var buff in region.RegionName.GetRegionInfo().RegionBuffs)
                    obj.ApplyModifyUnitBuff(unit.Coords, buff.BuffName, BuffOperation.RemoveBuff, buff.Count, false, true);
                Hook.RegionUnitLeave(coords, obj, unit, coords, false);
            }

            ExitSimulation();
        }
    }
}