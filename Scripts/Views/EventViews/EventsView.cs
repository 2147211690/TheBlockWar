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
    public BlockDraw BlockDraw { get; }
    private HashSet<long> _clearUnitIds = new();
    private List<ViewSprite2D> _usedTextures = new();
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