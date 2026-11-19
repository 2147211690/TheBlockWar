using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public class 火域Region() : Region(RegionNames.火域)
{
    public override void OnUnitEnter(SimObject obj, Vector2I regionCoords, string regionData, 
        IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion)
    {
        obj.ApplyModifyUnitBuff(unit.Coords, BuffNames.燃烧, BuffOperation.AddBuff, 1, false);
    }

    public override void OnEnterTurn(SimObject obj, Vector2I regionCoords, string regionData, int turn)
    {
        obj.ApplyModifyUnitBuff(regionCoords, BuffNames.燃烧, BuffOperation.AddBuff, 1, false);
    }
}