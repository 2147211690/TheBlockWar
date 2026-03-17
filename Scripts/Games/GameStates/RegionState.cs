namespace TheBlockWar.Scripts.Games.GameStates;

public struct RegionState
{
    public RegionTypeEnum RegionType { get; set; }
    /// <summary>
    /// 回合为单位
    /// </summary>
    public int Duration { get; set; }
}