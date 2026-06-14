using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step(["Valid"], [typeof(bool)])]
        public ApplyPlaceTileResult ApplyPlaceTile(Vector2I coords, string tileName)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyPlaceTileResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyPlaceTileResult(CurrentDeep, coords, tileName, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                if (!obj.State.TileMapState.IsEmpty(coords)) return r;
                r.Valid = true;
                obj.State.TileMapState.ModifyTile(coords, t =>
                {
                    t.Name = tileName;
                });
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
                return r;
            }
        }
    }
    public static partial void ApplyPlaceTileViewDraw(GameView view, ReadOnlySimObject sim, ApplyPlaceTileResult result)
    {
    }
    
    public static partial GDTask ApplyPlaceTileViewRun(GameView view, ReadOnlySimObject sim, ApplyPlaceTileResult result)
    {
        if (!result.Valid) return GDTask.CompletedTask;
        return view.TilesView.PlaceTile(result.Step.Coords, result.Step.TileName);
    }
}