using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long TargetIndex:int  Valid:bool
        [Step(["UnitId", "TargetIndex"], [typeof(long), typeof(int)])]
        public ApplyMoveUnitPathResult ApplyMoveUnitPath(Vector2I coords, List<Vector2I>? path)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyMoveUnitPathResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyMoveUnitPathResult(CurrentDeep, CurrentTeam, coords, path, 0, 0));
                if (path is not {Count: > 1}) return r;
                var unit = obj.State.UnitMapState.GetUnit(coords + path[0]);
                r.ReUnitId = unit.Id;
                if (unit.IsNull())
                {
                    return r;
                }

                var targetIndex = 1;

                for (; targetIndex < path.Count - 1; targetIndex++)
                    if (!unit.IsCanPass(obj.State, coords + path[targetIndex]))
                        break;

                for (; targetIndex >= 0; targetIndex--)
                    if (unit.IsCanBeDestination(obj.State, coords + path[targetIndex]))
                        break;

                r.ReTargetIndex = targetIndex;
                
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }
                
                if (targetIndex > 0) obj.ApplyMoveUnit(coords + path[0], coords + path[targetIndex]);
                return r;
            }
        }
    }

    public static partial void ApplyMoveUnitPathViewDraw(GameView view, ReadOnlySimObject sim,
        ApplyMoveUnitPathResult result)
    {
        var path = result.Path;
        if (path is not {Count: >= 2}) return;
        var maxTileHeight = 0f;
        foreach (var coord in path) 
            maxTileHeight = Mathf.Max(maxTileHeight, view.TilesView.GetTileHeight(coord + result.Coords));
        var tex = view.Style[TeamTypeEnum.Player].PathTextures[(int)(path[1] - path[0]).ToVecDirMask()];
        SetSprite(result.Coords + path[0], tex, 0);
        for (int i = 1; i < path.Count - 1; i++)
        {
            tex = view.Style[TeamTypeEnum.Player]
                .PathTextures[
                    (int)(path[i - 1] - path[i]).ToVecDirMask() |
                    (int)(path[i + 1] - path[i]).ToVecDirMask()];
            SetSprite(result.Coords + path[i], tex, i);
        }

        tex = view.Style[TeamTypeEnum.Player].PathTextures[(int)DirMask.All ^ (int)(path[^1] - path[^2]).ToVecDirMask()];
        SetSprite(result.Coords + path[^1], tex, path.Count - 1);

        void SetSprite(in Vector2I coords, Texture2D tex, int index)
        {
            var sprite = view.EventsView.GetGameTexture();
            sprite.GamePosition = view.TilesView.GetGamePosition(coords, maxTileHeight);
            sprite.Texture = tex;
            sprite.Modulate = Colors.White with { A = sim.IsFocused ? 1 : 0.6f };
            sprite.GameZIndex = -1;
        }
    }

    public static partial GDTask ApplyMoveUnitPathViewRun(GameView view, ReadOnlySimObject sim, ApplyMoveUnitPathResult result)
    {
        return view.UnitsView.MovePathUnit(result.ReUnitId, result.Coords, result.Path,
                result.ReTargetIndex);
    }
}