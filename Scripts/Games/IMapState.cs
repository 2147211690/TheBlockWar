using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IMapState
{
    Vector2I Size { get; }
    TileState GetTile(Vector2I coords);
    void SetTile(Vector2I coords, in TileState state);
    void ModifyTile(Vector2I coords, Func<TileState, TileState> modifier);
    IMapState GetSnapshot();
}