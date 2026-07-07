namespace TheBlockWar.Scripts.Games.GameStates;

public interface ITurnState : IReadOnlyTurnState, IGameDataState
{
    new int CurrentTurn { get; set; }
    Dictionary<int, Dictionary<string, int>> PhaseOrders { get; }
}