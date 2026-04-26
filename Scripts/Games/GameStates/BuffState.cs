using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Scripts.Games.GameStates;

public struct 
    BuffState
{
    public string BuffName { get; set; }
    /// <summary>
    /// 回合为单位
    /// </summary>
    public int Duration { get; set; }
}