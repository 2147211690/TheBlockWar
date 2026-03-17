using System.Collections.Generic;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileState
{
    TileTag Tag { get; }
    string Name { get; }
    bool IsOccupied { get; }
    IReadOnlyList<RegionState> Regions { get; }
    TileState Clone();
    TileState CopyTo(TileState tile);
}