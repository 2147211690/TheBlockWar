using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyRegionTurnResult ApplyRegionTurn(int turn)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyRegionTurnResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyRegionTurnResult(CurrentDeep, turn));
                if (obj.Layer == SimulationLayer.Waiting) return r;
                var regionPairs = new List<(Vector2I coords, IReadOnlyList<RegionState> regions)>();
                var size = obj.State.TileMapState.Size;
                for (var i = 0; i < size.X; i++)
                {
                    for (var j = 0; j < size.Y; j++)
                    {
                        var coords = new Vector2I(i, j);
                        var tile = obj.State.TileMapState.GetTile(coords);
                        if (tile.Regions.Count > 0)
                            regionPairs.Add(new(coords, tile.RegionsStatic));
                    }
                }
                obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
                foreach (var pair in regionPairs)
                {
                    Hook.RegionEnterTurn(pair.coords, obj, turn);
                }

                foreach (var pair in regionPairs)
                {
                    var tile = obj.State.TileMapState.GetTile(pair.coords);
                    foreach (var region in pair.regions)
                    {
                        if (tile.GetRegionDuration(region.RegionName) is 0 or int.MaxValue) continue;
                        obj.ApplyModifyTileRegion(pair.coords, region.RegionName, RegionOperation.ChangeDuration, -1);
                    }
                }
                obj.EndSteps();
                return r;
            }
        }
    }
    public static partial void ApplyRegionTurnViewDraw(GameView view, ReadOnlySimObject sim, ApplyRegionTurnResult result)
    {
    }
    
    public static partial GDTask ApplyRegionTurnViewRun(GameView view, ReadOnlySimObject sim, ApplyRegionTurnResult result)
    {
        return GDTask.CompletedTask;
    }
}