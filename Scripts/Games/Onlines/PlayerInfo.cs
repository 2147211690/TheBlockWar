namespace TheBlockWar.Scripts.Games.Onlines;
public class PlayerInfo
{
    public string Name { get; set; }
    public PlayerType Type { get; set; }
}

public enum PlayerType
{
    OnlinePlayer,
    AiPlayer,
}