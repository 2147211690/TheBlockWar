namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class TileStateData()
{
    public TileTag Tag { set; get; }
    public string Name { set; get; } = string.Empty;
    public List<RegionState>? Regions { set; get; }
}