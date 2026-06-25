using System.Linq;

namespace TheBlockWar.Scripts.Games;

public class RandomMapBuilder : MapBuilder
{
    public RandomMapBuilder(IEnumerable<string> tileNames, IEnumerable<string> unitNames, (int Team, int UnitCount)[] unitCounts, int seed)
    {
        TileNames = tileNames.ToArray();
        UnitNames = unitNames.ToArray();
        UnitCounts = unitCounts;
        _random = new Random(seed);
    }

    public string[] UnitNames { get; set; }
    public string[] TileNames { get; set; }
    public (int Team, int UnitCount)[] UnitCounts { get; set; }
    private Random _random;

    public override MapBuildResult BuildMap(Vector2I size)
    {
        var r = new MapBuildResult();
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                r.Tiles[new(i, j)] = TileNames[_random.Next(TileNames.Length)];
            }
        }
        PlaceUnits(r, size);
        return r;
    }

    private void PlaceUnits(MapBuildResult result, Vector2I size)
    {
        var occupied = new HashSet<Vector2I>();
        foreach (var (team, count) in UnitCounts)
        {
            for (int i = 0; i < count; i++)
            {
                Vector2I pos;
                do
                {
                    pos = new Vector2I(_random.Next(size.X), _random.Next(size.Y));
                } while (occupied.Contains(pos));
                occupied.Add(pos);
                result.Units[pos] = (UnitNames[_random.Next(UnitNames.Length)], team);
            }
        }
    }
}