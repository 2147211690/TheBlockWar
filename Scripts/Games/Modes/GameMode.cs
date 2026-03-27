using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Turns;

namespace TheBlockWar.Scripts.Games.Modes;

public abstract class GameMode : IGameMode
{
    protected static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public GameMode(){}
    public GameStateEnum State { get; protected set; } = GameStateEnum.Paused;
    
    public abstract void EnterTurn(TurnManager turnManager, int turn);

    public abstract GDTask Turn(TurnManager turnManager, int turn);

    public abstract void ExitTurn(TurnManager turnManager, int turn);
    
    protected async GDTask RunBuffRegionEventAndAddToNextTurnEvent(TurnManager turnManager, int turn, int delay = 800)
    {

    }

    protected SchedulerEvent GetBuffRegionEvent(int turn)
    {
        var e = new ActionSchedulerEvent()
        {
            ApplyAction = e =>
            {
                var regionPairs = new List<(Vector2I coords, IRegion[] regions)>();
                var size = e.State.TileMapState.Size;
                for (var i = 0; i < size.X; i++)
                {
                    for (var j = 0; j < size.Y; j++)
                    {
                        var coords = new Vector2I(i, j);
                        var tile = e.State.TileMapState.GetTile(coords);
                        if (tile.Regions.Count > 0)
                            regionPairs.Add(
                                new(coords,
                                    tile.Regions.Select(r => Regions.Regions.GetInstance(r.RegionType)).ToArray()));
                    }
                }

                foreach (var pair in regionPairs)
                {
                    foreach (var region in pair.regions)
                    {
                        region.OnEnterTurn(e.State, pair.coords, turn, e.Result);
                    }
                }


                List<(IReadOnlyUnitState unit, IBuff[] buffs)> buffPairs =
                    e.State.UnitMapState.GetAllUnits()
                        .Where(a => a.Buffs.Count > 0)
                        .Select(a => (a, a.Buffs.Select(u => u.BuffType.GetInstance()).ToArray()))
                        .ToList();
                foreach (var pair in buffPairs)
                {
                    foreach (var buff in pair.buffs)
                    {
                        buff.OnEnterTurn(e.State, pair.unit, turn, e.Result);
                    }
                }

                GC.Collect();
            },
            Turn = turn,
            Priority = int.MinValue,
        };
        return e;
    }
}