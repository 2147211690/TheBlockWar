using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileMapState
{
    public IReadOnlyTileMapState? Owner { get; }
    Vector2I Size { get; }
    IReadOnlyTileState GetTile(Vector2I coords);
    bool IsEmpty(Vector2I coords);
    ITileMapState GetSnapshot();
}