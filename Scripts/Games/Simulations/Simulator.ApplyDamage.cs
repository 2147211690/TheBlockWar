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
        public void ApplyDamage(Vector2I coords, float damage)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyDamageResult(CurrentDeep, coords, damage, 0, 0));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                float validDamage = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddResult(
                        new ApplyDamageResult(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
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

                if (unit.IsNull())
                {
                    obj.Steps?.AddResult(
                        new ApplyDamageResult(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
                }

                validDamage = Mathf.Min(rawDamage, unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    obj.Steps?.AddResult(
                        new ApplyDamageResult(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
                }

                obj.Steps?.AddResult(
                    new ApplyDamageResult(CurrentDeep, coords, damage, unit.Id, validDamage));

                // 触发受伤后事件, 仅在运行层触发
                if (validDamage > 0 && obj.ComplexSimulationRegion(0))
                {
                    // 触发受伤后事件
                    Hook.BuffUnitAfterGetDamage(unit, obj, validDamage);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (!unit.IsNull() && validDamage > 0 && unit.Health <= 0)
                {
                    obj.ApplyDeath(coords);
                }
            }
        }
    }
    
    public static partial void ApplyDamageViewDraw(GameView view, ReadOnlySimObject sim, ApplyDamageResult result)
    {
    }
    
    public static partial GDTask ApplyDamageViewRun(GameView view, ReadOnlySimObject sim, ApplyDamageResult result)
    {
        view.UnitsView.GetUnitView(result.UnitId)?.HealthBar.CurrentHealth -= result.ValidDamage;
        return GDTask.CompletedTask;
    }
}