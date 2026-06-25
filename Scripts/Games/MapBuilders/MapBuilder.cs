namespace TheBlockWar.Scripts.Games;

public abstract class MapBuilder
{
    public abstract MapBuildResult BuildMap(Vector2I size);
}

public class MapBuildResult
{
    public Dictionary<Vector2I, (string Name, int Team)> Units { get; } = new();
    public Dictionary<Vector2I, string> Tiles { get; } = new();
    public string? GetTile(Vector2I pos)
    {
        return Tiles.TryGetValue(pos, out var tile) ? tile : null;
    }
    public (string Name, int Team)? GetUnit(Vector2I pos)
    {
        return Units.TryGetValue(pos, out var unit) ? unit : null;
    }
}