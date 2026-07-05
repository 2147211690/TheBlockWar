using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;
/// <summary>
/// 用来构建地图的回合
/// </summary>
public class MapBuildPhase : TurnPhase
{
    public MapBuildPhase(MapBuilder mapBuilder)
    {
        MapBuilder = mapBuilder;
    }
    public override string PhaseName { get; } = "MapBuildPhase";
    public override bool IsFocused { get; set; }
    public MapBuilder MapBuilder { get; }
    public override GDTask Execute()
    {
        var size = EventScheduler.GameState.Size;
        var result = MapBuilder.BuildMap(size);
        EventScheduler.RunAction((es, obj) =>
        {
            obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
            for (int i = 0; i < size.X; i++)
            {
                obj.BeginSteps(100 * i, 0, StepsRunningMode.ParallelWaitAll);
                for (int j = 0; j < size.Y; j++)
                {
                    var tile = result.GetTile(new(i, j));
                    if (tile == null) continue;
                    obj.ApplyPlaceTile(new(i, j), tile);
                }
                obj.EndSteps();
            }
            obj.EndSteps();
            foreach (var (coords, (unitName, team)) in result.Units)
            {
                obj.ApplyPlaceUnit(coords, unitName, team);
            }
        });
        return EventScheduler.WaitCompleteRunning();
    }

    protected override void OnInit()
    {
    }
}