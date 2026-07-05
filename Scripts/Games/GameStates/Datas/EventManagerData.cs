namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class EventManagerData
{
    public required Dictionary<long, EventState> Events { get; init; }
    public required Vector2I Size { get; init; }
}