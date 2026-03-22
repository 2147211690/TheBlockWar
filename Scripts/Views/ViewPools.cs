using Godot;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;

namespace TheBlockWar.Scripts.Views;

public class ViewPools
{
    public GameViewStyle Style { get; set; }
    public ViewPools(Node2D drawContainer, GameViewStyle styleStyle)
    {
        DrawContainer = drawContainer;
        Style = styleStyle;
        AnimatedSprite2DPool = new TypedPool<AnimatedSprite2D>(() =>
        {
            var node = new AnimatedSprite2D();
            DrawContainer.AddChild(node);
            return node;
        }, 0)
        {
            AfterPush = u =>
            {
                u.Visible = false;
                u.ProcessMode = Node.ProcessModeEnum.Disabled;
            },
            BeforePop = u =>
            {
                u.Visible = true;
                u.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
        Sprite2DPool = new TypedPool<Sprite2D>(() =>
        {
            var node = new Sprite2D();
            DrawContainer.AddChild(node);
            return node;
        }, 0)
        {
            AfterPush = u =>
            {
                u.Visible = false;
                u.ProcessMode = Node.ProcessModeEnum.Disabled;
            },
            BeforePop = u =>
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
            AfterPush = a =>
            {
                a.Visible = false;
                a.DrawAction = null;
                a.ProcessRedraw = false;
                a.Args = null;
                a.Reset();
                a.ProcessMode = Node.ProcessModeEnum.Disabled;
            },
            BeforePop = a =>
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
            AfterPush = a =>
            {
                a.Visible = false;
                a.ProcessMode = Node.ProcessModeEnum.Disabled;
            },
            BeforePop = a =>
            {
                a.Visible = true;
                a.ProcessMode = Node.ProcessModeEnum.Inherit;
            }
        };
    }
    public Node2D DrawContainer { get; }
    public TypedPool<AnimatedSprite2D> AnimatedSprite2DPool { get; private set; }
    public TypedPool<Sprite2D> Sprite2DPool { get; private set; }
    public TypedPool<DrawNode> DrawNodePool { get; private set; }
    public TypedPool<BlockEventDrawNode> BlockEventDrawNodePool { get; private set; }
}