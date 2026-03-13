using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface ITilesState : IReadOnlyTilesState
{
    void ModifyTile(Vector2I coords, Action<TileState> modifier);
}