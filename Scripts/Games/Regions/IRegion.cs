using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public interface IRegion
{
    string Name { get; }
    //TODO:可能废弃,完全使用对应Buff替代
    void OnUnitEnter(SimObject obj, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I enterCoords,
        bool isAddRegion);
    void OnUnitLeave(SimObject obj, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords,
        bool isRemoveRegion);
    void OnEnterTurn(SimObject obj, Vector2I regionCoords, int turn);
}