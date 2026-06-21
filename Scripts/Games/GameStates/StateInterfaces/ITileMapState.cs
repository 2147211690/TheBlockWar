namespace TheBlockWar.Scripts.Games.GameStates;

public interface ITileMapState : IReadOnlyTileMapState, IGameDataState
{
    void ModifyTile(Vector2I coords, Action<TileState> modifier);
}