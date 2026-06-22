using System.Linq;
using TheBlockWar.Scripts.Games.Players;

namespace TheBlockWar.Scripts.Games.Turns;

public class PvpTurnStrategy : ITurnStrategy
{
    public PvpTurnStrategy(IPlayer[] players)
    {
        Players = (IPlayer[])players.Clone();
    }
    public IPlayer[] Players { get; }
    public IEnumerable<ITurnPhase> GetTurnPhases(TurnManager turnManager, int turn)
    {
        return
        [
            new BuffPhase(),
            new RegionPhase(),
            ..Players.Select(p => new TeamSkillPhase(p.Team)),
            new ClearOutdateEventPhase(),
            new ResetUnitPhase(),
            ..Players.Select(player => new PlayerPhase(player)),
        ];
    }
}