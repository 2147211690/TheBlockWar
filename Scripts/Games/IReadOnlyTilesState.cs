using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyTilesState
{
    Vector2I Size { get; }
    IReadOnlyTileState GetTile(Vector2I coords);
    ITilesState GetSnapshot();
}