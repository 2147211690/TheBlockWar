using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Games.GameStates;

public struct RegionState
{
    public string RegionName { get; set; }
    /// <summary>
    /// 回合为单位
    /// </summary>
    public int Duration { get; set; }
    public string RegionData { get; set; }
}