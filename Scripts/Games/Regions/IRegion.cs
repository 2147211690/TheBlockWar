using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public interface IRegion
{
    string Name { get; }    
    void OnUnitEnter(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationContext context);
    void OnUnitLeave(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationContext context);
    void OnEnterTurn(IGameState state, Vector2I regionCoords, int turn, SimulationContext context);
}