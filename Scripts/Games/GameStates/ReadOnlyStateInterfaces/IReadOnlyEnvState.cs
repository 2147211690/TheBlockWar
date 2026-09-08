using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyEnvState : IReadOnlyGameDataState<IEnvState, EnvStateData>
{
    IEnumerable<EnvStepState> EnvSteps { get; }
    IEnumerable<EnvStepState> GetEnvSteps(Vector2I coords);
}