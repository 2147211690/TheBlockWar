using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TeamState
{
    private int[,] _teamRelations;
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
            TeamRelations = teamRelations
        };
    }
    public bool IsEnemy(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == (int)TeamTypeEnum.Enemy;
    }
    public bool IsFriend(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == (int)TeamTypeEnum.Friend;
    }
    public bool IsPlayer(int team1, int team2)
    {
        return team1 == team2;
    }
    public bool IsPlayerOrFriend(int team1, int team2)
    {
        return IsPlayer(team1, team2) || IsFriend(team1, team2);
    }
    public bool IsNeutral(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == (int)TeamTypeEnum.Neutral;
    }
    public TeamTypeEnum GetTeamTypeRelativeToPlayer(int playerTeam, int team)
    {
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
}