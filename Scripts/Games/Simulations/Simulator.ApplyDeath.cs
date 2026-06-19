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
        public ApplyDeathResult ApplyDeath(Vector2I coords, bool draw = true)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyDeathResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyDeathResult(CurrentDeep, CurrentTeam, coords, draw,0, false));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.ReUnitId = unit.Id;

                if (unit.IsNull() || unit.IsDead)
                {
                    return r;
                }
                r.ReValid = true;
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health = 0);
                
                EnterSimulation();
                Hook.BuffUnitDeath(unit, obj);
                //移除所有buff
                foreach (var buff in unit.BuffsStatic)
                {
                    buff.BuffName.GetBuffInfo().Hook.OnRemoveFromUnit(obj, unit, buff.Count, buff.Count);
                }
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.ClearBuff());
                ExitSimulation();
                //TODO:可能引入区域相关事件
                return r;
            }
        }
    }
    public static partial void ApplyDeathViewDraw(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
        if (result.Draw)
        {
            var tex = view.EventsView.GetGameTexture();
            tex.Texture = view.CurrentStyle.DeathTexture;
            tex.GamePosition = result.Coords.ToUnderVec3(view);
            tex.GameZIndex = 3;
        }
        
        var uv = view.UnitsView.GetUnitView(result.ReUnitId);
        if (uv == null) return;
        uv.HealthBar.WillDie = true;
        view.EventsView.NeedClearUnit(result.ReUnitId);
    }
    
    public static partial GDTask ApplyDeathViewRun(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
        var uv = view.UnitsView.GetUnitView(result.ReUnitId);
        if (uv == null) return GDTask.CompletedTask;
        uv.HealthBar.CurrentHealth = 0;
        uv.IsDead = true;
        return GDTask.CompletedTask;
    }
}