using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyUnitBuff(Vector2I coords, BuffTypeEnum buffType, BuffOperation operation, 
            int duration, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
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
                        BuffOperation.AddBuff => state.HandleAddOrChangeBuff(unit, buffType, duration, true),
                        BuffOperation.RemoveBuff => state.HandleRemoveBuff(unit, buffType),
                        BuffOperation.ChangeDuration =>
                            state.HandleAddOrChangeBuff(unit, buffType, duration * 2, false),
                        BuffOperation.SetDuration => state.HandleSetDurationBuff(unit, buffType, duration),
                        _ => (BuffOperation.None, duration, 0)
                    };
                }

                result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
                    duration, unit.Id, resultOperation, resultDuration));

                if (ComplexSimulationRegion(result, 1))
                {
                    switch (resultOperation)
                    {
                        case BuffOperation.AddBuff:
                            state.NotifyUnitBuffs(unit, b => b.OnAddToUnit(state, unit, result));
                            break;
                        case BuffOperation.RemoveBuff:
                            state.NotifyUnitBuff(unit, buffType, b => b.OnRemoveFromUnit(state, unit, result));
                            break;
                        case BuffOperation.ChangeDuration:
                            state.NotifyUnitBuff(unit, buffType, b => b.OnDurationChanged(state, unit,
                                resultOldDuration, resultOldDuration + resultDuration, result));
                            break;
                    }
                }
            }
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleAddOrChangeBuff(IReadOnlyUnitState unit, 
            BuffTypeEnum buffType, int newDuration, bool isAdd)
        {
            if (unit.TryFindBuff(buffType, out var index))
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
            
            state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleRemoveBuff(IReadOnlyUnitState unit, 
            BuffTypeEnum buffType)
        {
            if (unit.TryFindBuff(buffType, out var index))
            {
                state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                return (BuffOperation.RemoveBuff, 0, 0);
            }
            return (BuffOperation.None, 0, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleSetDurationBuff(IReadOnlyUnitState unit, 
            BuffTypeEnum buffType, int newDuration)
        {
            if (unit.TryFindBuff(buffType, out var index))
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
            
            state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }
    }
}