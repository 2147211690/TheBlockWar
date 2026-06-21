using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileMapState : IReadOnlyGameDataState<ITileMapState, TileMapStateData>
{
    public IReadOnlyTileMapState? Owner { get; }
    Vector2I Size { get; }
    IReadOnlyTileState GetTile(Vector2I coords);
    IEnumerable<IReadOnlyTileState> GetAllTiles();
    bool IsEmpty(Vector2I coords);
}