using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //TODO:可能使用事件优化
        public void ApplyDamage(Vector2I coords, float damage)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new DamageStep(CurrentDeep, coords, damage, 0, 0));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                float validDamage = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddStep(
                        new DamageStep(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
                }
                var originDamage = damage;
                var rawDamage = damage;

                if (obj.ComplexSimulationRegion(0))
                {
                    //应用来源单位伤害前事件
                    var source = obj.State.UnitMapState.GetUnit(CurrentContext.SourceUnitId);
                    obj.NotifyUnitBuffs(source,
                        b => b.OnBeforeDamageToUnit(obj, source, unit, originDamage, ref rawDamage));
                    //应用单位伤害前事件
                    obj.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeGetDamage(obj, unit, originDamage, ref rawDamage));
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    obj.Steps?.AddStep(
                        new DamageStep(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
                }

                validDamage = Mathf.Min(rawDamage, unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    obj.Steps?.AddStep(
                        new DamageStep(CurrentDeep, coords, damage, unit.Id, validDamage));
                    return;
                }

                obj.Steps?.AddStep(new DamageStep(CurrentDeep, coords, damage, unit.Id, validDamage));

                // 触发受伤后事件, 仅在运行层触发
                if (validDamage > 0 && obj.ComplexSimulationRegion(0))
                {
                    // 触发受伤后事件
                    obj.NotifyUnitBuffs(unit, b => b.OnUnitAfterGetDamage(obj, unit, validDamage));
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (!unit.IsNull() && validDamage > 0 && unit.Health <= 0)
                {
                    obj.ApplyDeath(coords);
                }
            }
        }
    }
}