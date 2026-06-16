using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyTileState
{
    public static readonly float MaxTileHeight = 34;
    string Name { get; }
    IReadOnlyList<RegionState> Regions { get; }
    IReadOnlyList<RegionState> RegionsStatic { get; }
    int FindRegion(string regionName);
    TileState Clone();
    TileState CopyTo(TileState tile);
    TileStateData ToData();
}