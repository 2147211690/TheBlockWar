using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyDamage(Vector2I coords, float damage, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, 0, 0));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                float validDamage = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                var rawDamage = damage;

                //应用单位伤害前事件
                if (ComplexSimulationRegion(result, 0))
                {
                    state.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeDamage(state, unit, rawDamage, ref rawDamage, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                validDamage = Mathf.Min(rawDamage, unit.Health);
                state.UnitMapState.ModifyUnit(unit.Id, u => u.Health -= validDamage);
                unit = state.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));
                    return;
                }

                result.Steps?.AddStep(new DamageStep(IsCurrentChildSimulation, coords, damage, unit.Id, validDamage));

                // 触发受伤后事件, 仅在运行层触发
                if (validDamage > 0 && ComplexSimulationRegion(result, 0))
                {
                    // 触发受伤后事件
                    state.NotifyUnitBuffs(unit, b => b.OnUnitAfterDamage(state, unit, validDamage, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (!unit.IsNull() && unit.Health <= 0)
                {
                    ApplyDeath(state, coords, result);
                }
            }
        }
    }
}