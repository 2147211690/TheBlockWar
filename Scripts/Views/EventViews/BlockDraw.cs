using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.EventViews;

public class BlockDraw
{
    private EventsView _eventsView;
    private TypedPool<BlockEventDrawNode> Pool => _eventsView.GameView.ViewPools.BlockEventDrawNodePool;
    private Dictionary<Vector2I, BlockEventDrawNode> _nodes = new();

    public BlockDraw(EventsView eventsView)
    {
        _eventsView = eventsView;
    }

    public void AddHealthChange(Vector2I coords, float originChange, float validChange, bool isFocused, TeamTypeEnum team)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddHealthChange(originChange, validChange, isFocused, team);
    }

    public void AddBuffChange(Vector2I coords, BuffTypeEnum buffType, bool isAdd, bool isFocused, TeamTypeEnum team)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddBuffChange(buffType, isAdd, isFocused, team);
    }

    public void AddRegionChange(Vector2I coords, RegionTypeEnum regionType, bool isAdd, bool isFocused, TeamTypeEnum team)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddRegionChange(regionType, isAdd, isFocused, team);
    }

    private BlockEventDrawNode GetBlockEventDrawNode(Vector2I coords)
    {
        if (_nodes.TryGetValue(coords, out var node)) return node;
        node = Pool.Pop();
        node.ZIndex = 2;
        _nodes.Add(coords, node);
        node.Position = _eventsView.GameView.TilesView.GetRealPosition(coords, Layer.Sky);

        return node;
    }

    public void Clear()
    {
        foreach (var node in _nodes.Values)
        {
            node.Clear();
            Pool.Push(node);
        }
        _nodes.Clear();
    }
}