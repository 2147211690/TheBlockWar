using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public interface IRegion
{
    RegionInfo Info { get; }
    void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationResult result);
    void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationResult result);
}