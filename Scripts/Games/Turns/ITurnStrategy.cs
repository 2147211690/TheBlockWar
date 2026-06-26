namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnStrategy
{
    IEnumerable<ITurnPhase> GetTurnPhases(ITurnManager turnManager, int turn);
}