namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class GameStateData
{
    public Vector2I Size { set; get; }
    public DateTime BaseTime { get; set; }
    public required TileMapStateData TileMapState { get; init; }
    public required UnitMapStateData UnitMapState { get; init; }
    public required TeamStateData TeamState { get; init; }
}