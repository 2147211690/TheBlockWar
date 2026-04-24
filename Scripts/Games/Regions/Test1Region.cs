using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Regions;

public class Test1Region() : Region("res://Resources/Infos/RegionInfos/Test1Region.tres")
{
    public override void OnUnitEnter(IGameState state, Vector2I regionCoords,  IReadOnlyUnitState unit, Vector2I enterCoords, 
        bool isAddRegion, SimulationContext context)
    {
        Logger.Info($"Test1 OnUnitEnter {unit} {enterCoords}");
        state.ApplyPushUnit(enterCoords, DirMask.Right, context);
    }

    public override void OnUnitLeave(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords, 
        bool isRemoveRegion, SimulationContext context)
    {
        Logger.Info($"Test1 OnUnitLeave {unit} {leaveCoords}");
        state.ApplyDamage(unit.Coords, 30, context);
    }
}