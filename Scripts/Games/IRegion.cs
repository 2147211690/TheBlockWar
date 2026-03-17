using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public interface IRegion
{
    RegionTypeEnum RegionType { get; }
    bool IsOnce { get; }
    int Duration { get; }
    void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationResult result);
    void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationResult result);
}