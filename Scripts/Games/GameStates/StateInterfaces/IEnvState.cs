using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IEnvState : IReadOnlyEnvState, IGameDataState
{ 
    void AddEnvStep(Vector2I coords, int priority, ApplyStep step);
    void ClearEnvSteps();
}