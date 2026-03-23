using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyCure(Vector2I coords, float cure, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new CureStep(IsCurrentChildSimulation, coords, cure, 0, 0));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                float validCure = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    result.Steps?.AddStep(
                        new CureStep(IsCurrentChildSimulation, coords, cure, unit.Id, validCure));
                    return;
                }

                var rawCure = cure;

                if (ComplexSimulationRegion(result, 0))
                {
                    state.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeCure(state, unit, rawCure, ref rawCure, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new CureStep(IsCurrentChildSimulation, coords, cure, unit.Id, validCure));
                    return;
                }

                validCure = Mathf.Min(rawCure, unit.MaxHealth - unit.Health);
                state.UnitMapState.ModifyUnit(unit.Id, u => u.Health += validCure);
                unit = state.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    result.Steps?.AddStep(
                        new CureStep(IsCurrentChildSimulation, coords, cure, unit.Id, validCure));
                    return;
                }

                result.Steps?.AddStep(new CureStep(IsCurrentChildSimulation, coords, cure, unit.Id, validCure));

                if (validCure > 0 && ComplexSimulationRegion(result, 0))
                {
                    state.NotifyUnitBuffs(unit, b => b.OnUnitAfterCure(state, unit, validCure, result));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }
            }
        }
    }
}