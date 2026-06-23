using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyDropTileResult ApplyDropTile(Vector2I coords)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyDropTileResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyDropTileResult(CurrentDeep, CurrentTeam, coords));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                
                obj.State.TileMapState.ModifyTile(coords, t => TileState.Empty.CopyTo(t));
                if (!unit.IsNull())
                {
                    if (unit.IsWillDrop(obj.State, coords))
                        obj.ApplyDropUnit(coords);
                    else
                    {
                        obj.ApplyUnitRemoveFromCoords(unit, coords);
                    }
                }
                return r;
            }
        }
    }

    public static partial void ApplyDropTileReplayCore(IGameState state, ApplyDropTileResult result)
    {
        state.TileMapState.ModifyTile(result.Coords, t => TileState.Empty.CopyTo(t));
    }
    public static partial void ApplyDropTileViewDraw(IGameView view, ReadOnlySimObject sim, ApplyDropTileResult result)
    {
    }
    
    public static partial async GDTask ApplyDropTileViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyDropTileResult result)
    {
        var tv = view.GetTileView(result.Coords);
        if (tv == null) return;
        await AnimationHelper.DropToAsync(tv, tv.GamePosition + new Vector3(0, 0, -40));
        view.SetTileView(result.Coords, "Air"); 
    }
}