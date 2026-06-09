using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

public class ViewPools
{
    public GameViewStyle Style { get; set; }
    public ViewPools(Node2D drawContainer, GameViewStyle styleStyle)
    {
        DrawContainer = drawContainer;
        Style = styleStyle;
        AnimatedSprite2DPool = new TypedPool<ViewAnimatedSprite2D>(() =>
        {
            var node = new ViewAnimatedSprite2D();
            DrawContainer.AddChild(node);
            return node;
        }, 0)
        {
            AfterReturn = u =>
            {
                u.Visible = false;
                u.ProcessMode = Node.ProcessModeEnum.Disabled;
                u.GameZIndex = 0;
                u.NodeZIndexOffset = 0;
                u.Offset = Vector2.Zero;
                u.Scale = Vector2.One;
                u.GameRotationDegrees = 0;
            },
            BeforeGet = u =>
            {
                u.Visible = true;
                u.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
        Sprite2DPool = new TypedPool<ViewSprite2D>(() =>
        {
            var node = new ViewSprite2D();
            DrawContainer.AddChild(node);
            return node;
        }, 0)
        {
            AfterReturn = u =>
            {
                u.Visible = false;
                u.ProcessMode = Node.ProcessModeEnum.Disabled;
                u.GameZIndex = 0;
                u.NodeZIndexOffset = 0;
                u.Offset = Vector2.Zero;
                u.Scale = Vector2.One;
                u.GameRotationDegrees = 0;
            },
            BeforeGet = u =>
            {
                u.Visible = true;
                u.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
        DrawNodePool = new TypedPool<DrawNode>(() =>
        {
            var node = new DrawNode();
            DrawContainer.AddChild(node);
            return node;
        }, 20)
        {
            AfterReturn = a =>
            {
                a.Visible = false;
                a.DrawAction = null;
                a.ProcessRedraw = false;
                a.Args = null;
                a.Reset();
                a.ProcessMode = Node.ProcessModeEnum.Disabled;
                a.GameZIndex = 0;
                a.NodeZIndexOffset = 0;
            },
            BeforeGet = a =>
            {
                a.Visible = true;
                a.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
        BlockEventDrawNodePool = new TypedPool<BlockEventDrawNode>(() =>
        {
            var node = new BlockEventDrawNode();
            node.Style = Style;
            DrawContainer.AddChild(node);
            node.Visible = false;
            return node;
        }, 10)
        {
            AfterReturn = a =>
            {
                a.Visible = false;
                a.ProcessMode = Node.ProcessModeEnum.Disabled;
                a.GameZIndex = 0;
                a.NodeZIndexOffset = 0;
            },
            BeforeGet = a =>
            {
                a.Visible = true;
                a.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
    }
    public Node2D DrawContainer { get; }
    public TypedPool<ViewAnimatedSprite2D> AnimatedSprite2DPool { get; private set; }
    public TypedPool<ViewSprite2D> Sprite2DPool { get; private set; }
    public TypedPool<DrawNode> DrawNodePool { get; private set; }
    public TypedPool<BlockEventDrawNode> BlockEventDrawNodePool { get; private set; }
}