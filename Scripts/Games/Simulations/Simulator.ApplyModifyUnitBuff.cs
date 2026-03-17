using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyModifyUnitBuff(Vector2I coords, BuffTypeEnum buffType, BuffOperation operation, int duration,
            SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();


            void ChangeUnitValidBuff(IReadOnlyUnitState unit, int changeIndex, int oldDuration, int newDuration,
                out BuffOperation resultOperation, out int resultDuration)
            {
                if (newDuration == 0)
                {
                    //如果新持续时间为0，则移除buff
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(changeIndex));
                    resultOperation = BuffOperation.RemoveBuff;
                    resultDuration = 0;
                }
                else
                {
                    state.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(changeIndex, newDuration));
                    var change = newDuration - oldDuration;
                    resultOperation = BuffOperation.ChangeDuration;
                    resultDuration = change;
                }
            }

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
                        duration,
                        0, operation, duration));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;
                if (!unit.IsNull())
                {
                    switch (operation)
                    {
                        case BuffOperation.AddBuff: //添加buff
                            if (unit.TryFindBuff(buffType, out var addIndex))
                            {
                                //如果buff已存在，则使用最大持续时间
                                var oldDuration = unit.Buffs[addIndex].Duration;
                                var newDuration = Mathf.Max(duration, oldDuration);
                                ChangeUnitValidBuff(unit, addIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                        case BuffOperation.RemoveBuff: //移除buff
                            if (unit.TryFindBuff(buffType, out var removeIndex))
                            {
                                //如果buff已存在
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(removeIndex));
                                resultOperation = BuffOperation.RemoveBuff;
                                resultDuration = 0;
                            }

                            break;
                        case BuffOperation.ChangeDuration: //改变buff持续时间
                            if (unit.TryFindBuff(buffType, out var changeIndex))
                            {
                                var oldDuration = unit.Buffs[changeIndex].Duration;
                                var newDuration = Mathf.Max(duration + duration, 0);
                                //如果buff已存在
                                ChangeUnitValidBuff(unit, changeIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                                resultOldDuration = oldDuration;
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                        case BuffOperation.SetDuration: //设置buff持续时间
                            if (unit.TryFindBuff(buffType, out var setIndex))
                            {
                                //如果buff已存在
                                var oldDuration = unit.Buffs[setIndex].Duration;
                                var newDuration = Mathf.Max(duration, 0);
                                ChangeUnitValidBuff(unit, setIndex, oldDuration, newDuration,
                                    out resultOperation, out resultDuration);
                            }
                            else
                            {
                                //如果buff不存在，则直接添加
                                state.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffType, duration));
                                resultOperation = BuffOperation.AddBuff;
                                resultDuration = duration;
                            }

                            break;
                    }
                }

                result.Steps?.AddStep(new ModifyUnitBuffStep(IsCurrentChildSimulation, coords, buffType, operation,
                    duration,
                    unit.Id, resultOperation, resultDuration));

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
                        default:
                            break;
                    }
                }
            }
        }
    }
}