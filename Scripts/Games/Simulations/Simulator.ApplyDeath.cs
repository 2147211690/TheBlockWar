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
        public ApplyDeathResult ApplyDeath(Vector2I coords)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyDeathResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyDeathResult(CurrentDeep, coords, 0, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.UnitId = unit.Id;

                if (unit.IsNull() || unit.IsDead)
                {
                    return r;
                }
                r.Valid = true;
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health = 0);
                Hook.BuffUnitDeath(unit, obj);
                Hook.BuffRemoveFromUnit(unit, obj);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.ClearBuff());
                //TODO:可能引入区域相关事件
                return r;
            }
        }
    }
    public static partial void ApplyDeathViewDraw(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
        var uv = view.UnitsView.GetUnitView(result.UnitId);
        if (uv == null) return;
        uv.HealthBar.WillDie = true;
        view.EventsView.NeedClearUnit(result.UnitId);
    }
    
    public static partial GDTask ApplyDeathViewRun(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
        view.UnitsView.GetUnitView(result.UnitId)?.HealthBar.CurrentHealth = 0;
        view.UnitsView.GetUnitView(result.UnitId)?.IsDead = true;
        return GDTask.CompletedTask;
    }
}