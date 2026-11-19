using TheBlockWar.Scripts.Games.Turns.Phases;

namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnStrategy
{
    IEnumerable<ITurnPhase> GetTurnPhases(ITurnManager turnManager, int turn);
}