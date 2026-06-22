namespace TheBlockWar.Scripts.Games.Turns;

public interface ITurnStrategy
{
    IEnumerable<ITurnPhase> GetTurnPhases(TurnManager turnManager, int turn);
}