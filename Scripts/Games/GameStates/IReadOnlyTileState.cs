using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileState
{
    string Name { get; }
    IReadOnlyList<RegionState> Regions { get; }
    IReadOnlyList<RegionState> RegionsStatic { get; }
    TileState Clone();
    TileState CopyTo(TileState tile);
    TileStateData ToData();
}