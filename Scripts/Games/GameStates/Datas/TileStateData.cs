using TheBlockWar.Scripts.Games.Tiles;

namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class TileStateData()
{
    public TileTag Tag { set; get; }
    public TileNameEnum Name { set; get; }
    public List<RegionState>? Regions { set; get; }
}