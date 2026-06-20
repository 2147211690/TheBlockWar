using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.UnitViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class EventsView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public IGameView GameView { get; set; } = null!;
    private TypedPool<ViewSprite2D> SpritePool => GameView.Pools.Sprite2DPool;
    private TypedPool<DrawNode> DrawNodePool => GameView.Pools.DrawNodePool;
    public BlockDraw BlockDraw { get; }
    private HashSet<UnitView> _clearUnitViews = new();
    private List<ViewSprite2D> _usedTextures = new();
    private List<DrawNode> _usedDrawNodes = new();
    public EventsView()
    {
        BlockDraw = new(this);
    }

    public ViewSprite2D EventSprite()
    {
        var sprite = SpritePool.Get();
        _usedTextures.Add(sprite);
        return sprite;
    }
    public UnitView? EventUnit(long id)
    {
        var uv = GameView.GetUnitView(id);
        if (uv == null) return null;
        _clearUnitViews.Add(uv);
        return uv;
    }

    public void EventTopDraw(Action<ViewNode2D> action, int zOffset = 0)
    {
        var n = GetDrawNode();
        n.DrawAction = (node, b) =>
        {
            if (b) action.Invoke(node);
        };
        n.GameZIndex = zOffset;
        n.NodeZIndexOffset = 3000;
        n.Position = new(0, 0);
    }
    
    public ViewNode2D EventDraw(Action<ViewNode2D> action)
    {
        var n = GetDrawNode();
        n.DrawAction = (node, b) =>
        {
            if (b) action.Invoke(node);
        };
        return n;
    }
    private DrawNode GetDrawNode()
    {
        var drawNode = DrawNodePool.Get();
        _usedDrawNodes.Add(drawNode);
        return drawNode;
    }

    /*public void DrawPointLine(Func<Vector3, Vector3, float, Vector3> line, Vector3 start, Vector3 end, float split,
        Color color)
    {
        var nVec = (end - start);
        nVec.Z /= GameView.NormalGridHeight;
        var length = nVec.Length();
        if (length < split) return;
        var step = 1 / Mathf.Round(length / split);
        var node = GetDrawNode();
        node.NodeZIndexOffset = 4000;
        node.Position = new(0, 0);
        node.DrawAction = (drawNode, b) =>
        {
            if (b)
            {
                for (var i = 0f; i <= 1f + step / 2; i += step)
                {
                    var pos = line(start, end, i);
                    drawNode.DrawTextureCentered(GameView.CurrentStyle.LinePointTexture,
                        -drawNode.GameToNodePosition(pos, 0), color);
                }
            }
        };
    }*/
    public void EventClear()
    {
        BlockDraw.Clear();
        foreach (var sprite in _usedTextures)
        {
            SpritePool.Return(sprite);
        }
        _usedTextures.Clear();
        foreach (var drawNode in _usedDrawNodes)
        {
            DrawNodePool.Return(drawNode);
        }
        _usedDrawNodes.Clear();
        foreach (var uv in _clearUnitViews)
        {
            uv.EventClear();
        }
        _clearUnitViews.Clear();
    }
}