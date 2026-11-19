namespace TheBlockWar.Scripts.Games.GameStates;

public interface ITurnState : IReadOnlyTurnState, IGameDataState
{
    new int CurrentTurn { get; set; }
}