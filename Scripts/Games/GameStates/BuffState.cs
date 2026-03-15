namespace TheBlockWar.Scripts.Games.GameStates;

public struct BuffState
{
    public BuffTypeEnum BuffType { get; set; }
    /// <summary>
    /// 回合为单位
    /// </summary>
    public int Duration { get; set; }
}