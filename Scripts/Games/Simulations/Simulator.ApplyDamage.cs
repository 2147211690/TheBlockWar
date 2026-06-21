using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //TODO:可能使用事件优化
        //result:UnitId:long ValidDamage:float
        [Step(["UnitId", "ValidDamage"],[typeof(long), typeof(float)])]
        public ApplyDamageResult ApplyDamage(Vector2I coords, float damage)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyDamageResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyDamageResult(CurrentDeep, CurrentTeam, coords, damage, 0, 0));
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
                var originDamage = damage;
                var rawDamage = damage;

                if (obj.ComplexSimulationRegion(0))
                {
                    //应用来源单位伤害前事件
                    var source = obj.State.UnitMapState.GetUnit(CurrentContext.SourceUnitId);
                    Hook.BuffBeforeDamageToUnit(source, obj, unit, originDamage, ref rawDamage);
                    //应用单位伤害前事件
                    var origin = rawDamage;
                    Hook.BuffUnitBeforeGetDamage(unit, obj, origin, ref rawDamage);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsDead)
                {
                    return r;
                }

                var validDamage = Mathf.Min(rawDamage, unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);
                r.ReValidDamage = validDamage;

                // 触发受伤后事件
                if (validDamage > 0 && obj.ComplexSimulationRegion(0))
                {
                    Hook.BuffUnitAfterGetDamage(unit, obj, validDamage);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (!unit.IsDead && validDamage > 0 && unit.Health <= 0)
                {
                    obj.ApplyDeath(coords, false);
                }
                return r;
            }
        }
    }
    
    public static partial void ApplyDamageViewDraw(IGameView view, ReadOnlySimObject sim, ApplyDamageResult result)
    {
        view.EventHealth(result.Coords, -result.Damage, -result.ReValidDamage, 
            result.ReUnitId != 0 ,sim.IsFocused);
        var uv = view.EventUnit(result.ReUnitId);
        if (uv == null) return;
        uv.HealthBar.EventHealthChange -= result.ReValidDamage;
    }
    
    public static partial GDTask ApplyDamageViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyDamageResult result)
    {
        var uv = view.GetUnitView(result.ReUnitId);
        if (uv == null || uv.IsDead) return GDTask.CompletedTask;
        uv.HealthBar.CurrentHealth -= result.ReValidDamage;
        AnimationHelper.HurtShakeAsync(uv).Forget();
        return GDTask.CompletedTask;
    }
}