namespace TheBlockWar.Scripts.Views.ViewNodes;

[GlobalClass]
public partial class ViewNode2D : Node2D, IGameViewNode<Node2D>
{
    private readonly GameViewNode<Node2D> _gameViewNode;
    public ViewNode2D()
    {
        _gameViewNode = new GameViewNode<Node2D>(this);
    }
    public GameViewNodeContainer? Container => _gameViewNode.Container;

    public Node2D Node => _gameViewNode.Node;

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
}