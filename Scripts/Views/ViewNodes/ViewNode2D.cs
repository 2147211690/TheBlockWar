namespace TheBlockWar.Scripts.Views.ViewNodes;

[GlobalClass]
public partial class ViewNode2D : Node2D, IGameViewNode<Node2D>
{
    private readonly GameViewNode<Node2D> _gameViewNode;
    public ViewNode2D()
    {
        _gameViewNode = new GameViewNode<Node2D>(this);
    }
    public ViewNode2D(Node2D renderCenter)
    {
        _gameViewNode = new GameViewNode<Node2D>(this, renderCenter);
    }
    public GameViewNodeContainer? Container => _gameViewNode.Container;

    public Node2D Node => _gameViewNode.Node;
    public virtual Node2D RenderNode => _gameViewNode.RenderNode;

    public Vector3 GamePosition
    {
        get => _gameViewNode.GamePosition;
        set => _gameViewNode.GamePosition = value;
    }

    public float GameRotationDegrees
    {
        get => _gameViewNode.GameRotationDegrees;
        set => _gameViewNode.GameRotationDegrees = value;
    }

    public float NodeRotation
    {
        get => _gameViewNode.NodeRotation;
        set => _gameViewNode.NodeRotation = value;
    }

    public int GameZIndex
    {
        get => _gameViewNode.GameZIndex;
        set => _gameViewNode.GameZIndex = value;
    }

    public int NodeZIndexOffset
    {
        get => _gameViewNode.NodeZIndexOffset;
        set => _gameViewNode.NodeZIndexOffset = value;
    }

    public Vector2 GameToNodePosition(Vector3 gamePosition, int zIndex)
    {
        return _gameViewNode.GameToNodePosition(gamePosition, zIndex);
    }
    /// <summary>
    /// 使用游戏坐标设置节点旋转
    /// </summary>
    /// <param name="xyAngle">XY平面旋转角度,关于X正方向</param>
    /// <param name="xzAngle">高度平面旋转角度,XY平面</param>
    public void SetGameRotation(float xyAngle, float xzAngle)
    {
        _gameViewNode.SetGameRotation(xyAngle, xzAngle);
    }

    public void SetGameRotation(Vector3 direction)
    {
        _gameViewNode.SetGameRotation(direction);
    }

    public virtual void Reset()
    {
        _gameViewNode.Reset();
    }
}