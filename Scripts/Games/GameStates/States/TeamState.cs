using System.Linq;
using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TeamState : ITeamState
{
    private int[,] _teamRelations;

    /// <summary>
    /// 各队伍待放置单位槽位:队伍 → 按 index 的待放置列表。值为 null 表示该槽位无待放置。
    /// </summary>
    public Dictionary<int, List<PreparePlaceUnitInfo?>> PreparePlaces { get; } = new();

    /// <summary>
    /// 0-Player
    /// 1-Enemy
    /// 2-Neutral
    /// 3-Friend
    /// </summary>
    /// <param name="teamRelations"></param>
    public TeamState(int[,] teamRelations)
    {
        _teamRelations = (int[,])teamRelations.Clone();
    }

    public TeamState(TeamStateData teamStateData)
    {
        var x = teamStateData.TeamRelations.Length;
        var y = teamStateData.TeamRelations[0].Length;
        _teamRelations = new int[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                _teamRelations[i, j] = teamStateData.TeamRelations[i][j];
            }
        }

        if (teamStateData.PreparePlaces is not null)
        {
            foreach (var (team, places) in teamStateData.PreparePlaces)
            {
                PreparePlaces[team] = places.ToList();
            }
        }
    }

    public TeamStateData ToData()
    {
        var teamRelations = new int[_teamRelations.GetLength(0)][];
        for (int i = 0; i < _teamRelations.GetLength(0); i++)
        {
            teamRelations[i] = new int[_teamRelations.GetLength(1)];
            for (int j = 0; j < _teamRelations.GetLength(1); j++)
            {
                teamRelations[i][j] = _teamRelations[i, j];
            }
        }

        return new TeamStateData
        {
            TeamRelations = teamRelations,
            PreparePlaces = PreparePlaces.ToDictionary(
                kv => kv.Key, kv => (List<PreparePlaceUnitInfo?>)kv.Value.ToList())
        };
    }

    /// <summary>读取某队伍待放置单位槽位列表(不存在则返回空列表)。</summary>
    public IReadOnlyList<PreparePlaceUnitInfo?> GetTeamPreparePlaceUnits(int team)
        => PreparePlaces.TryGetValue(team, out var list) ? list : Array.Empty<PreparePlaceUnitInfo?>();

    /// <summary>枚举所有含待放置记录的队伍(仅含至少一个非空槽位)。</summary>
    public IEnumerable<int> GetPrepareTeams()
    {
        foreach (var (team, list) in PreparePlaces)
        {
            if (list.Any(p => p.HasValue)) yield return team;
        }
    }

    public int[,] TeamRelations => _teamRelations;

    /// <summary>读取某队伍第 index 个待放置单位槽位(越界或无则返回 null)。</summary>
    public PreparePlaceUnitInfo? GetTeamPreparePlaceUnit(int team, int index)
    {
        if (!PreparePlaces.TryGetValue(team, out var list)) return null;
        if (index < 0 || index >= list.Count) return null;
        return list[index];
    }

    /// <summary>写入某队伍第 index 个待放置单位槽位(自动扩容);null 表示将该槽位置空。</summary>
    public void SetTeamPreparePlaceUnit(int team, int index, PreparePlaceUnitInfo? placeInfo)
    {
        if (!PreparePlaces.TryGetValue(team, out var list))
        {
            PreparePlaces[team] = list = new List<PreparePlaceUnitInfo?>();
        }

        while (list.Count <= index) list.Add(null);
        list[index] = placeInfo;
    }

    /// <summary>清空某队伍全部待放置单位槽位。</summary>
    public void ClearTeamPreparePlaceUnits(int team)
    {
        PreparePlaces.Remove(team);
    }

    public bool IsEnemy(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == (int)TeamTypeEnum.Enemy;
    }

    public ITeamState GetSnapshot() => new TeamStateSnapshot(this);

    public void ClearIfSnapshot()
    {
    }

    public bool IsPlayer(int team1, int team2)
    {
        return team1 == team2;
    }

    public TeamTypeEnum GetTeamTypeRelativeToPlayer(int playerTeam, int team)
    {
        if (team < 0) return TeamTypeEnum.Enemy;
        if (IsPlayer(playerTeam, team)) return TeamTypeEnum.Player;
        var relation = TryGetRelation(playerTeam, team);
        return (TeamTypeEnum)relation;
    }

    private int TryGetRelation(int team1, int team2)
    {
        if (team1 < 0 || team1 >= _teamRelations.GetLength(0) ||
            team2 < 0 || team2 >= _teamRelations.GetLength(1)) return (int)TeamTypeEnum.Enemy;
        return _teamRelations[team1, team2];
    }






    public sealed class TeamStateSnapshot : ITeamState
    {
        private readonly IReadOnlyTeamState _source;
        private readonly Dictionary<int, List<PreparePlaceUnitInfo?>> _places = new();

        public TeamStateSnapshot(IReadOnlyTeamState source)
        {
            _source = source;
            foreach (var team in source.GetPrepareTeams())
            {
                var list = source.GetTeamPreparePlaceUnits(team);
                _places[team] = list.Count == 0
                    ? new List<PreparePlaceUnitInfo?>()
                    : new List<PreparePlaceUnitInfo?>(list);
            }
        }

        public int[,] TeamRelations => _source.TeamRelations;

        public bool IsEnemy(int team1, int team2) => _source.IsEnemy(team1, team2);
        public bool IsPlayer(int team1, int team2) => _source.IsPlayer(team1, team2);

        public TeamTypeEnum GetTeamTypeRelativeToPlayer(int playerTeam, int team) =>
            _source.GetTeamTypeRelativeToPlayer(playerTeam, team);

        public IEnumerable<int> GetPrepareTeams() => _places.Keys;

        public IReadOnlyList<PreparePlaceUnitInfo?> GetTeamPreparePlaceUnits(int team)
            => _places.TryGetValue(team, out var list) ? list : Array.Empty<PreparePlaceUnitInfo?>();

        public PreparePlaceUnitInfo? GetTeamPreparePlaceUnit(int team, int index)
        {
            if (!_places.TryGetValue(team, out var list)) return null;
            if (index < 0 || index >= list.Count) return null;
            return list[index];
        }

        public void SetTeamPreparePlaceUnit(int team, int index, PreparePlaceUnitInfo? placeInfo)
        {
            if (!_places.TryGetValue(team, out var list))
            {
                _places[team] = list = new List<PreparePlaceUnitInfo?>();
            }

            while (list.Count <= index) list.Add(null);
            list[index] = placeInfo;
        }

        public void ClearTeamPreparePlaceUnits(int team) => _places.Remove(team);

        public ITeamState GetSnapshot() => new TeamStateSnapshot(this);

        public void ClearIfSnapshot()
        {
            _places.Clear();
            foreach (var team in _source.GetPrepareTeams())
            {
                var list = _source.GetTeamPreparePlaceUnits(team);
                _places[team] = list.Count == 0
                    ? new List<PreparePlaceUnitInfo?>()
                    : new List<PreparePlaceUnitInfo?>(list);
            }
        }

        public TeamStateData ToData()
        {
            var relations = TeamRelations;
            var rows = new int[relations.GetLength(0)][];
            for (var i = 0; i < rows.Length; i++)
            {
                rows[i] = new int[relations.GetLength(1)];
                for (var j = 0; j < rows[i].Length; j++) rows[i][j] = relations[i, j];
            }

            return new TeamStateData
            {
                TeamRelations = rows,
                PreparePlaces = _places.ToDictionary(kv => kv.Key,
                    kv => (List<PreparePlaceUnitInfo?>)kv.Value.ToList()),
            };
        }
    }
}