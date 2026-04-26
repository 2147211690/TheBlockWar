using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Games.GameStates;

public struct RegionState
{
    public RegionNameEnum RegionName { get; set; }
    /// <summary>
    /// 回合为单位
    /// </summary>
    public int Duration { get; set; }
}