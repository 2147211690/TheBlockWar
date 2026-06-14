using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class EventsView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public GameView GameView { get; set; } = null!;
    private GameViewNodeContainer DrawContainer => GameView.DrawContainer;
    private TypedPool<ViewSprite2D> SpritePool => GameView.ViewPools.Sprite2DPool;
    private TypedPool<DrawNode> DrawNodePool => GameView.ViewPools.DrawNodePool;
    public BlockDraw BlockDraw { get; }
    private HashSet<long> _clearUnitIds = new();
    private List<ViewSprite2D> _usedTextures = new();
    private List<DrawNode> _usedDrawNodes = new();
    public EventsView()
    {
        BlockDraw = new(this);
    }

    public ViewSprite2D GetGameTexture()
    {
        var sprite = SpritePool.Get();
        _usedTextures.Add(sprite);
        return sprite;
    }
    public DrawNode GetDrawNode()
    {
        var drawNode = DrawNodePool.Get();
        _usedDrawNodes.Add(drawNode);
        return drawNode;
    }

    public void DrawPointLine(Func<Vector3, Vector3, float, Vector3> line, Vector3 start, Vector3 end, float split,
        Color color)
    {
        var nVec = (end - start);
        nVec.Z /= GameView.NormalHeight;
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
    }

    public void SetUnitNeedClear(long id)
    {
        _clearUnitIds.Add(id);
    }
    public void ClearAll()
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
        foreach (var id in _clearUnitIds)
        {
            var view = GameView.UnitsView.GetUnitView(id);
            if (view is null) continue;
            view.ClearFacks();
            view.HealthBar.ClearAllChanges();
            view.BuffView.ClearAllBuffChanges();
            view.UnitsView.SetUnitAimCoords(id, null);
        }
        _clearUnitIds.Clear();
    }
    public void NeedClearUnit(long id)
    {
        _clearUnitIds.Add(id);
    }
}