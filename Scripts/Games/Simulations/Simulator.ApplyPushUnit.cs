using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        public void ApplyPushUnit(Vector2I coords, DirMask dir)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new PushUnitStep(CurrentDeep, coords, dir, 0, true));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                var target = coords + dir.ToVec2I();
                var targetUnit = obj.State.UnitMapState.GetUnit(target);
                var validPush = !unit.IsNull() && targetUnit.IsNull() && obj.State.IsInMap(target);
                var shouldPush = true;
                Hook.BuffUnitBeforePush(unit, obj, dir, ref shouldPush);
                obj.Steps?.AddStep(new PushUnitStep(CurrentDeep, coords, dir, unit.Id, validPush && shouldPush));
                if (unit.IsNull() || !shouldPush) return;
                if (validPush)
                {
                    obj.ApplyMoveUnit(coords, target);
                }
                else
                {
                    //推动碰撞伤害是互相给予的
                    PushContext(new SimContext(targetUnit.Id));
                    obj.ApplyDamage(coords, 10); 
                    PopContext();
                    PushContext(new SimContext(unit.Id));
                    if (!targetUnit.IsNull()) obj.ApplyDamage(target, 10);
                    PopContext();
                }
            }
        }
    }
}