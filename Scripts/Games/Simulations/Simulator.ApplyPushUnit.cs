using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId", "Valid"], [typeof(long), typeof(bool)])]
        public ApplyPushUnitResult ApplyPushUnit(Vector2I coords, DirMask dir)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyPushUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyPushUnitResult(
                    CurrentDeep, CurrentTeam, coords, dir, 0, true));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.ReUnitId = unit.Id;
                var target = coords + dir.ToVec2I();
                var targetUnit = obj.State.UnitMapState.GetUnit(target);
                var validPush = !unit.IsNull() && targetUnit.IsNull() && obj.State.IsInMap(target);
                var shouldPush = true;
                Hook.BuffUnitBeforePush(unit, obj, dir, ref shouldPush);
                r.ReValid = validPush && shouldPush;
                if (unit.IsNull() || !shouldPush) return r;
                if (validPush)
                {
                    obj.ApplyMoveUnit(coords, target);
                }
                else
                {
                    //推动碰撞伤害是互相给予的
                    obj.PushContext(new SimContext(targetUnit.Id));
                    obj.ApplyDamage(coords, 10); 
                    obj.PopContext();
                    obj.PushContext(new SimContext(unit.Id));
                    if (!targetUnit.IsNull()) obj.ApplyDamage(target, 10);
                    obj.PopContext();
                }
                return r;
            }
        }
    }

    public static partial void ApplyPushUnitReplayCore(IGameState state, ApplyPushUnitResult result)
    {
    }
    
    public static partial void ApplyPushUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyPushUnitResult result)
    {
        var s = view.EventSprite();
        var pos = result.Coords.ToVec2() + result.Dir.ToVec2() * (result.ReValid ? 0.5f : 0.2f);
        s.GamePosition = pos.ToVec3(view.GetTileHeight(result.Coords));
        s.Texture = view.CurrentStyle.PushTextures[result.Dir.ToIndex()];
        s.Modulate = (sim.IsFocused , result.ReValid) switch
        {
            (true, true) => Colors.Yellow,
            (true, false) => Colors.White,
            _ => new Color(1,1,1,0.5f),
        };
    }

    public static partial GDTask ApplyPushUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyPushUnitResult result)
    {
        var uv = view.GetUnitView(result.ReUnitId);
        if (uv == null) return GDTask.CompletedTask;
        var targetPos = view.CoordsToGamePosition(result.Coords + result.Dir.ToVec2I());
        if (result.ReValid)
        {
            return AnimationHelper.PushToAsync(uv, targetPos);
        }
        else
        {
            return AnimationHelper.PushBounceAsync(uv, targetPos);
        }
    }
}