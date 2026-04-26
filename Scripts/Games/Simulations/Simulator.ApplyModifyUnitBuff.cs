using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyUnitBuff(Vector2I coords, string buffName, BuffOperation operation, 
            int duration, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new ModifyUnitBuffStep(CurrentDeep, coords, buffName, operation,
                        duration, 0, operation, duration));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;

                if (!unit.IsNull())
                {
                    (resultOperation, resultDuration, resultOldDuration) = operation switch
                    {
                        BuffOperation.AddBuff => state.HandleAddOrChangeBuff(unit, buffName, duration, true),
                        BuffOperation.RemoveBuff => state.HandleRemoveBuff(unit, buffName),
                        BuffOperation.ChangeDuration =>
                            state.HandleAddOrChangeBuff(unit, buffName, duration * 2, false),
                        BuffOperation.SetDuration => state.HandleSetDurationBuff(unit, buffName, duration),
                        _ => (BuffOperation.None, duration, 0)
                    };
                }

                context.Steps?.AddStep(new ModifyUnitBuffStep(CurrentDeep, coords, buffName, operation,
                    duration, unit.Id, resultOperation, resultDuration));

                if (ComplexSimulationRegion(context, 1))
                {
                    switch (resultOperation)
                    {
                        case BuffOperation.AddBuff:
                            state.NotifyUnitBuffs(unit, b => b.OnAddToUnit(state, unit, context));
                            break;
                        case BuffOperation.RemoveBuff:
                            state.NotifyUnitBuff(unit, buffName, b => b.OnRemoveFromUnit(state, unit, context));
                            break;
                        case BuffOperation.ChangeDuration:
                            state.NotifyUnitBuff(unit, buffName, b => b.OnDurationChanged(state, unit,
                                resultOldDuration, resultOldDuration + resultDuration, context));
                            break;
                    }
                }
            }
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleAddOrChangeBuff(IReadOnlyUnitState unit, 
            string buffName, int newDuration, bool isAdd)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                var oldDuration = unit.Buffs[index].Duration;
                var finalDuration = isAdd ? Mathf.Max(newDuration, oldDuration) : Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                    return (BuffOperation.RemoveBuff, 0, oldDuration);
                }
                
                state.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(index, finalDuration));
                return (BuffOperation.ChangeDuration, finalDuration - oldDuration, oldDuration);
            }
            
            state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffName, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleRemoveBuff(IReadOnlyUnitState unit, 
            string buffName)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                return (BuffOperation.RemoveBuff, 0, 0);
            }
            return (BuffOperation.None, 0, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleSetDurationBuff(IReadOnlyUnitState unit, 
            string buffName, int newDuration)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                var oldDuration = unit.Buffs[index].Duration;
                var finalDuration = Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                    return (BuffOperation.RemoveBuff, 0, oldDuration);
                }
                
                state.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(index, finalDuration));
                return (BuffOperation.ChangeDuration, finalDuration - oldDuration, oldDuration);
            }
            
            state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffName, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }
    }
}