namespace TheBlockWar.Scripts.Games.GameStates;

/// <summary>
/// 团队状态的完整视图:在只读基础上提供待放置单位槽位的修改。
/// <see cref="TeamStateSnapshot"/> 亦实现此接口,支持安全的 COW 快照。
/// </summary>
public interface ITeamState : IReadOnlyTeamState, IGameDataState
{
    /// <summary>写入某队伍第 index 个待放置单位槽位(自动扩容);null 表示将该槽位置空。</summary>
    void SetTeamPreparePlaceUnit(int team, int index, PreparePlaceUnitInfo? placeInfo);

    /// <summary>清空某队伍全部待放置单位槽位。</summary>
    void ClearTeamPreparePlaceUnits(int team);
}