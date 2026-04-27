using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class TileStateData()
{
    public string Name { set; get; } = string.Empty;
    public List<RegionState>? Regions { set; get; }
}