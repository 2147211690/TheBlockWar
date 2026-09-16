using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Regions;

public class 生成单位域Region() : Region(RegionNames.生成单位域)
{
    public override void OnEnterTurn(SimObject obj, Vector2I regionCoords, string regionData, int turn)
    {
        obj.ApplyPushUnit(regionCoords, DirMask.Down);
    }
}