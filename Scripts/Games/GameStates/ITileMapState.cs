using System;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface ITileMapState : IReadOnlyTileMapState
{
    void ModifyTile(Vector2I coords, Action<TileState> modifier);
    void ClearIfSnapshot();
}