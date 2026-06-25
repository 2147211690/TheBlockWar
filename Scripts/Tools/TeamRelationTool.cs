namespace TheBlockWar.Scripts.Tools;

public class TeamRelationTool
{
    public static int[,] GetAllTeamEnemy(int count)
    {
        var teamRelations = new int[count, count];
        for (int i = 0; i < count; i++)
        {
            for (int j = 0; j < count; j++)
            {
                teamRelations[i, j] = i == j ? 0 : 1;
            }
        }
        return teamRelations;
    }
}