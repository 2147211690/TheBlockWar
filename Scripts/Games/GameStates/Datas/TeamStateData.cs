namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class TeamStateData
{
    public required int[][] TeamRelations { get; init; }
    /// <summary>各队伍待放置单位槽位(队伍 → 按 index 列表)。</summary>
    public Dictionary<int, List<PreparePlaceUnitInfo?>>? PreparePlaces { get; init; }
}