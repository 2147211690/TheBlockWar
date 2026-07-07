using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public interface IRegion
{
    string Name { get; }
    [RegionHook]
    void OnUnitEnter(SimObject obj, Vector2I regionCoords, string regionData ,IReadOnlyUnitState unit, Vector2I enterCoords,
        bool isAddRegion);
    [RegionHook]
    void OnUnitLeave(SimObject obj, Vector2I regionCoords, string regionData, IReadOnlyUnitState unit, Vector2I leaveCoords,
        bool isRemoveRegion);
    [RegionHook]
    void OnEnterTurn(SimObject obj, Vector2I regionCoords, string regionData, int turn);
    [RegionHook]
    void OnAddToTile(SimObject obj, Vector2I regionCoords, string regionData);    
    [RegionHook]
    void OnRemoveFromTile(SimObject obj, Vector2I regionCoords, string regionData);
    [RegionHook]
    void OnCreateRegionEvent(SimObject obj, Vector2I regionCoords, string regionData, ref IEventCore eventCore);
}