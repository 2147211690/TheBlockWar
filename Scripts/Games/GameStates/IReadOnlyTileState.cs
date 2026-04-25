using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileState
{
    TileTag Tag { get; }
    string Name { get; }
    IReadOnlyList<RegionState> Regions { get; }
    TileState Clone();
    TileState CopyTo(TileState tile);
    TileStateData ToData();
}