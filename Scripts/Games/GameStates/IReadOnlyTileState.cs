using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Tiles;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileState
{
    TileTag Tag { get; }
    TileNameEnum Name { get; }
    IReadOnlyList<RegionState> Regions { get; }
    TileState Clone();
    TileState CopyTo(TileState tile);
    TileStateData ToData();
}