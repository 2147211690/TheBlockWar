using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.EventViews;

public class BlockDraw
{
    private EventsView _eventsView;
    private TypedPool<BlockEventDrawNode> Pool => _eventsView.GameView.Pools.BlockEventDrawNodePool;
    private Dictionary<Vector2I, BlockEventDrawNode> _nodes = new();

    public BlockDraw(EventsView eventsView)
    {
        _eventsView = eventsView;
    }

    public void AddHealthChange(Vector2I coords, float originChange, float validChange, bool hasAim, bool isFocused)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddHealthChange(originChange, validChange, hasAim, isFocused, _eventsView.GameView.CurrentTeamType);
    }

    public void AddBuffChange(Vector2I coords, string buffName, bool isAdd, bool isFocused)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddBuffChange(buffName, isAdd, isFocused, _eventsView.GameView.CurrentTeamType);
    }

    public void AddRegionChange(Vector2I coords, string regionName, bool isAdd, bool isFocused)
    {
        var node = GetBlockEventDrawNode(coords);
        node.Coords = coords;
        node.AddRegionChange(regionName, isAdd, isFocused, _eventsView.GameView.CurrentTeamType);
    }

    private BlockEventDrawNode GetBlockEventDrawNode(Vector2I coords)
    {
        if (_nodes.TryGetValue(coords, out var node)) return node;
        node = Pool.Get();
        node.NodeZIndexOffset = 4001;
        _nodes.Add(coords, node);
        node.GamePosition = _eventsView.GameView.CoordsToGamePosition(coords);

        return node;
    }

    public void Clear()
    {
        foreach (var node in _nodes.Values)
        {
            node.Clear();
            Pool.Return(node);
        }
        _nodes.Clear();
    }
}