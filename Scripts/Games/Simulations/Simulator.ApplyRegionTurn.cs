using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyRegionTurn(int turn)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                obj.Steps?.AddStep(new RegionTurnStep(CurrentDeep, turn));
                if (obj.Layer == SimulationLayer.Waiting) return;
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
            }
        }
    }
}