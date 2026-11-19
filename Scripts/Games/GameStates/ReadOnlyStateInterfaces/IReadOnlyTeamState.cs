using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

/// <summary>
/// 团队状态的只读视图:团队关系(假定不变)与待放置单位槽位的读取。
/// </summary>
public interface IReadOnlyTeamState : IReadOnlyGameDataState<ITeamState, TeamStateData>
{
    /// <summary>团队关系表(来源,快照不复制、直接共享——假定不变)。</summary>
    int[,] TeamRelations { get; }

    bool IsEnemy(int team1, int team2);
    bool IsPlayer(int team1, int team2);
    TeamTypeEnum GetTeamTypeRelativeToPlayer(int playerTeam, int team);

    /// <summary>枚举所有含待放置记录的队伍,供遍历。</summary>
    IEnumerable<int> GetPrepareTeams();
    IReadOnlyList<PreparePlaceUnitInfo?> GetTeamPreparePlaceUnits(int team);
    PreparePlaceUnitInfo? GetTeamPreparePlaceUnit(int team, int index);
}