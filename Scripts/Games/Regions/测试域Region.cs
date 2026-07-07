using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public class 测试域Region() : Region(RegionNames.测试域)
{
    public override void OnCreateRegionEvent(SimObject obj, Vector2I regionCoords, string regionData, ref IEventCore eventCore)
    {
        eventCore = new StepsEventCore(regionCoords,[
            new ApplyDamageStep(regionCoords, 50)
        ]);
    }
}