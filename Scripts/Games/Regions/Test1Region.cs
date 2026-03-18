using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

public class Test1Region() : Region("res://Resources/Infos/RegionInfos/Test1Region.tres")
{
    public override void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, 
        bool isAddRegion, SimulationResult result)
    {
        Logger.Info($"Test1 OnUnitEnter {unit} {enterCoords}");
        state.ApplyPushUnit(enterCoords, DirMask.Right, result);
    }

    public override void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, 
        bool isRemoveRegion, SimulationResult result)
    {
        Logger.Info($"Test1 OnUnitLeave {unit} {leaveCoords}");
        state.ApplyDamage(unit.Coords, 30, result);
    }
}