namespace TheBlockWar.Scripts.Games;

public class FlatMapBuilder : MapBuilder
{
    public FlatMapBuilder(string tileName)
    {
        TileName = tileName;
    }

    public string TileName { get; set; }
    public override MapBuildResult BuildMap(Vector2I size)
    {
        var r = new MapBuildResult();
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                r.Tiles[new(i, j)] = TileName;
            }
        }
        return r;
    }
}