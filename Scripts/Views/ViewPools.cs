using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;
using TheBlockWar.Scripts.Views.TileViews;
using TheBlockWar.Scripts.Views.UnitViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

public class ViewPools
{
    private Node2D _drawContainer;
    public ViewPools(Node2D drawContainer, PackedScene unitViewScene, PackedScene tileViewScene)
    {
        _drawContainer = drawContainer;
        
        AnimatedSprite2DPool = CreateFactory<ViewAnimatedSprite2D>();
        Sprite2DPool = CreateFactory<ViewSprite2D>(20);
        DrawNodePool = CreateFactory<DrawNode>(20);
        BlockEventDrawNodePool = CreateFactory<BlockEventDrawNode>(10);
        UnitViewPool = CreateFactory<UnitView>(unitViewScene,6);
        TileViewPool = CreateFactory<TileView>(tileViewScene,64);
    }
    
    private TypedPool<T> CreateFactory<T>(int capacity = 0) where T : Node2D, IGameViewNode<Node2D>, new()
        => CreateFactory(() => new T(), capacity);
    
    private TypedPool<T> CreateFactory<T>(PackedScene scene,int capacity = 0) where T : Node2D, IGameViewNode<Node2D>
        => CreateFactory(() => scene.Instantiate<T>(), capacity);
    
    private TypedPool<T> CreateFactory<T>(Func<T> factory, int capacity = 0) where T : Node2D, IGameViewNode<Node2D>
    {
        return new TypedPool<T>(() =>
        {
            var node = factory.Invoke();
            _drawContainer.AddChild(node);
            return node;
        }, capacity)
        {
            AfterReturn = u =>
            {
                u.Visible = false;
                u.ProcessMode = Node.ProcessModeEnum.Disabled;
                u.Reset();
            },
            BeforeGet = u =>
            {
                u.Visible = true;
                u.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
    }
    public TypedPool<ViewAnimatedSprite2D> AnimatedSprite2DPool { get; private set; }
    public TypedPool<UnitView> UnitViewPool { get; private set; }
    public TypedPool<TileView> TileViewPool { get; private set; }
    public TypedPool<ViewSprite2D> Sprite2DPool { get; private set; }
    public TypedPool<DrawNode> DrawNodePool { get; private set; }
    public TypedPool<BlockEventDrawNode> BlockEventDrawNodePool { get; private set; }
}