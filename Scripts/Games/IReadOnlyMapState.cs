using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IReadOnlyMapState
{
    Vector2I Size { get; }
    TileState GetTile(Vector2I coords);
    IMapState GetSnapshot();
}