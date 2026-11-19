using System.Linq;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Turns.Phases;

namespace TheBlockWar.Scripts.Games.Turns;

public class PvpTurnStrategy : ITurnStrategy
{
    private readonly IPlayerOperationSynchronizer _operationSynchronizer;
    private readonly MapBuilder _mapBuilder;
    public IPlayer[] Players { get; }

    public PvpTurnStrategy(IPlayerOperationSynchronizer operationSynchronizer, IPlayer[] players, MapBuilder mapBuilder)
    {
        _operationSynchronizer = operationSynchronizer;
        _mapBuilder = mapBuilder;
        Players = (IPlayer[])players.Clone();
        for (int i = 0; i < Players.Length; i++)
        {
            players[i].Team = i;
        }
    }
    public IEnumerable<ITurnPhase> GetTurnPhases(ITurnManager turnManager, int turn)
    {
        if (turn == 0)
        {
            return
            [
                new MapBuildPhase(_mapBuilder),
                new TestPhase(),
            ];
        }
        return GetNormalTurnPhases();

        IEnumerable<ITurnPhase> GetNormalTurnPhases()
        {
            return
            [
                new BuffPhase(),
                new RegionPhase(),
                ..Players.Select(p => new SkillRunPhase(p.Team, $"{p.Team}Phase")),
                new ResetUnitPhase(),
                ..Players.SelectMany<IPlayer, TurnPhase>(player => 
                    [/*new UnitPlacePhase(player.Team),*/
                        new PlayerPhase(_operationSynchronizer, player)]),
            ];
        }
    }
}