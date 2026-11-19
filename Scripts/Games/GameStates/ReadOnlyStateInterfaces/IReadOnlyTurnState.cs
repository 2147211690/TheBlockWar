using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTurnState : IReadOnlyGameDataState<ITurnState, TurnStateData>
{
    int CurrentTurn { get; }
    int GetPhaseOrder(int turn, string phaseName);
    Dictionary<int, Dictionary<string, int>> PhaseOrders { get; }
}