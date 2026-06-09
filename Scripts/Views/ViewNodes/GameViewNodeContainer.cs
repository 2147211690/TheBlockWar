namespace TheBlockWar.Scripts.Views.ViewNodes;
[GlobalClass]
public partial class GameViewNodeContainer : Node2D
{
    [Export] public GameView GameView { get; set; }
    public Vector2 GetNodePosition(Vector2 coords)
    {
        return GameView.TilesView.GamePlanePositionToNodePosition(coords);
    }
}