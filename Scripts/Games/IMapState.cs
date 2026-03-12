using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IMapState : IReadOnlyMapState
{
    void SetTile(Vector2I coords, in TileState state);
    void ModifyTile(Vector2I coords, Func<TileState, TileState> modifier);
}