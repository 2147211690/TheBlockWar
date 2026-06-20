namespace TheBlockWar.Scripts.Views.ViewNodes;

public interface IGameViewNode<out T> where T : Node2D
{
    GameViewNodeContainer? Container { get; }
    T Node { get; }
    Vector3 GamePosition { get; set; }
    float GameRotationDegrees { get; set; }
    float NodeRotation { get; set; }
    int GameZIndex { get; set; }
    int NodeZIndexOffset { get; set; }
    Vector2 GameToNodePosition(Vector3 gamePosition, int zIndex);
    void SetGameRotation(float xyAngle, float xzAngle);
    void SetGameRotation(Vector3 direction);
    void Reset();
}

public class GameViewNode<T> : IGameViewNode<T> where T : Node2D
{
    public GameViewNodeContainer? Container { get; private set; }
    public T Node { get; }

    public Vector3 GamePosition
    {
        get;
        set
        {
            field = value;
            UpdateNodePosition();
            UpdateNodeZIndex();
        }
    }

    public float GameRotationDegrees
    {
        get => Mathf.RadToDeg(NodeRotation);
        set => NodeRotation = Mathf.DegToRad(value);
    }
    /// <summary>
    /// 节点旋转角度,非游戏角度
    /// </summary>
    public float NodeRotation
    {
        get;
        set
        { 
            if (Math.Abs(field - value) < float.Epsilon) return;
            field = value;
            UpdateRotation();
        }
    }

    public int GameZIndex
    {
        get;
        set
        { 
            if (field == value) return;
            field = value;
            UpdateNodePosition();
        }
    }

    public int NodeZIndexOffset
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            UpdateNodeZIndex();
        }
    }

    public Vector2 GameToNodePosition(Vector3 gamePosition, int zIndex)
    {
        var pos = Container?.GameView.GamePositionToNodePosition(gamePosition) ?? Vector2.Zero;
        pos.Y += -zIndex * 0.01f;
        return -pos;
    }

    public GameViewNode(T node)
    {
        Node = node;
        Node.Rotation = Mathf.Pi;
        Node.TreeEntered += OnTreeEntered;
        Node.TreeExiting += OnTreeExiting;
    }
    /// <summary>
    /// 使用游戏坐标设置节点旋转
    /// </summary>
    public void SetGameRotation(float xyAngle, float xzAngle)
    {
        if (Container == null) return;
        var xzv = Mathf.Cos(xzAngle);
        var xzvh = Mathf.Sin(xzAngle);
        var xv = xzv * Mathf.Cos(xyAngle);
        var yv = xzv * Mathf.Sin(xyAngle);
        var viewVec =
            Container.GameView.NodeRight * xv + 
            Container.GameView.NodeDown * yv +
            new Vector2(0, -Container.GameView.NormalGridHeight) * xzvh;
        
        NodeRotation = Vector2.Right.AngleTo(viewVec);
    }

    public void SetGameRotation(Vector3 direction)
    {
        if (Container == null) return;
        var normal = direction.Normalized();
        var viewVec =
            Container.GameView.NodeRight * normal.X + 
            Container.GameView.NodeDown * normal.Y +
            Vector2.Up * normal.Z;
        
        NodeRotation = Vector2.Right.AngleTo(viewVec);
    }

    public void Reset()
    {
        GamePosition = Vector3.Zero;
        NodeRotation = 0;
        GameZIndex = 0;
        NodeZIndexOffset = 0;
        Node.Modulate = Colors.White;
        Node.Scale = Vector2.One;
    }

    private void UpdateRotation()
    {
        Node.Rotation =  NodeRotation - Mathf.Pi;
    }
    private void UpdateNodeZIndex()
    {
        Node.ZIndex = Mathf.CeilToInt(GamePosition.X + GamePosition.Y - 0.1f) + NodeZIndexOffset;
    }
    
    private void UpdateNodePosition()
    {
        Node.Position = GameToNodePosition(GamePosition, GameZIndex);
    }
    private void OnTreeExiting()
    {
        Container = null;
    }

    private void OnTreeEntered()
    {
        Container = Node.GetParentOrNull<GameViewNodeContainer>();
        UpdateRotation();
        UpdateNodePosition();
        UpdateNodeZIndex();
    }
}