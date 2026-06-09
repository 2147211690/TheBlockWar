namespace TheBlockWar.Scripts.Views.ViewNodes;
[GlobalClass]
public partial class ViewSprite2D : Sprite2D, IGameViewNode<Sprite2D>
{
    private readonly GameViewNode<Sprite2D> _gameViewNode;
    public ViewSprite2D()
    {
        _gameViewNode = new GameViewNode<Sprite2D>(this);
    }
    public GameViewNodeContainer? Container => _gameViewNode.Container;

    public Sprite2D Node => _gameViewNode.Node;

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