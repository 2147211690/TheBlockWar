namespace TheBlockWar.Scripts.Views.ViewNodes;

public interface IGameViewNode<out T> where T : Node2D
{
    GameViewNodeContainer? Container { get; }
    T Node { get; }
    Vector3 GamePosition { get; set; }
    float GameRotationDegrees { get; set; }
    int GameZIndex { get; set; }
    int NodeZIndexOffset { get; set; }
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
        get;
        set
        { 
            field = value;
            Node.RotationDegrees =  180 + value; 
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
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            UpdateNodeZIndex();
        }
    }
    
    public GameViewNode(T node)
    {
        Node = node;
        Node.RotationDegrees = 180;
        Node.TreeEntered += OnTreeEntered;
        Node.TreeExited += OnTreeExited;
    }

    private void UpdateNodeZIndex()
    {
        Node.ZIndex = Mathf.CeilToInt(GamePosition.X + GamePosition.Y - 0.1f) + NodeZIndexOffset;
    }
    
    private void UpdateNodePosition()
    {
        var s = new Vector2(GamePosition.X, GamePosition.Y);
        Node.Position = -((Container?.GetNodePosition(s) ?? s) + new Vector2(0, -(GamePosition.Z + GameZIndex * 0.01f)));
    }
    
    private void OnTreeExited()
    {
        Container = null;
    }

    private void OnTreeEntered()
    {
        Container = Node.GetParentOrNull<GameViewNodeContainer>();
    }
}