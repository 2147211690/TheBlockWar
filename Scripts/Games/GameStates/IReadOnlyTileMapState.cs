using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileMapState
{
    Vector2I Size { get; }
    IReadOnlyTileState GetTile(Vector2I coords);
    ITileMapState GetSnapshot();
}