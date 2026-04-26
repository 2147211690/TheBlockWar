using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyCure(Vector2I coords, float cure, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new CureStep(CurrentDeep, coords, cure, 0, 0));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                float validCure = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    context.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                var rawCure = cure;

                if (ComplexSimulationRegion(context, 0))
                {
                    state.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeCure(state, unit, rawCure, ref rawCure, context));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    context.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                validCure = Mathf.Min(rawCure, unit.MaxHealth - unit.Health);
                state.UnitMapState.ModifyUnit(unit.Id, u => u.Health += validCure);
                unit = state.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    context.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                context.Steps?.AddStep(new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));

                if (validCure > 0 && ComplexSimulationRegion(context, 0))
                {
                    state.NotifyUnitBuffs(unit, b => b.OnUnitAfterCure(state, unit, validCure, context));
                    unit = state.UnitMapState.GetUnit(unit.Id);
                }
            }
        }
    }
}