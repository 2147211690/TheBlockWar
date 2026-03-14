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
    public bool IsEnemy(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == 1;
    }
    public bool IsFriend(int team1, int team2)
    {
        return TryGetRelation(team1, team2) == 3;
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
        return TryGetRelation(team1, team2) == 2;
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
            team2 < 0 || team2 >= _teamRelations.GetLength(1)) return 1;
        return _teamRelations[team1, team2];
    }
}