using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Turns;

public class MapBuildPhase : ITurnPhase
{
    public MapBuildPhase(MapBuilder mapBuilder)
    {
        MapBuilder = mapBuilder;
    }

    public int Turn { get; private set; }
    public bool IsFocused { get; set; }
    public int PhaseOrder { get; private set; }
    public IEventScheduler EventScheduler { get; private set; }
    public TurnManager TurnManager { get; private set; }
    public MapBuilder MapBuilder { get; }
    public GDTask Execute()
    {
        var size = EventScheduler.GameState.Size;
        var result = MapBuilder.BuildMap(size);
        EventScheduler.RunAction(obj =>
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

    public ITurnPhase Init(TurnManager turnManager, int turn, IEventScheduler eventScheduler, int order)
    {
        Turn = turn;
        EventScheduler = eventScheduler;
        PhaseOrder = order;
        TurnManager = turnManager;
        return this;
    }
}