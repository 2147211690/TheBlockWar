using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //TODO:tile改为枚举
        public void ApplyPlaceTile(Vector2I coords, string tileName)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new PlaceTileStep(CurrentDeep, coords, tileName));
                    return;
                }

                obj.State.TileMapState.ModifyTile(coords, t =>
                {
                    t.Name = tileName;
                });
                obj.Steps?.AddStep(new PlaceTileStep(CurrentDeep, coords, tileName));
                if (obj.ComplexSimulationRegion(2))
                {
                    //初始化区域
                    var info = tileName.GetTileInfo();
                    var newTile = obj.State.TileMapState.GetTile(coords);
                    if (info.Regions is { Length: > 0 })
                    {
                        obj.State.TileMapState.ModifyTile(coords, t =>
                        {
                            foreach (var regionName in info.Regions) t.AddRegion(regionName, int.MaxValue);
                        });
                    }
                    //更新单位位置
                    var unit = obj.State.UnitMapState.GetUnit(coords);
                    if (!unit.IsNull()) obj.ApplyUnitSetInCoords(unit);
                }
            }
        }
    }
}