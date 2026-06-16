using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Operation:BuffOperation Duration:int
        [Step(["UnitId", "Operation", "Duration"], [typeof(long), typeof(BuffOperation), typeof(int)])]
        public ApplyModifyUnitBuffResult ApplyModifyUnitBuff(Vector2I coords, string buffName, BuffOperation operation, 
            int duration, bool draw = true)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;
            
            ApplyModifyUnitBuffResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyModifyUnitBuffResult(CurrentDeep, coords, buffName, operation,
                    duration, draw ,0, operation, duration));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.UnitId = unit.Id;
                var oldDuration = unit.GetBuffCount(buffName);
                var resultOperation = BuffOperation.None;
                var resultDuration = duration;
                var resultOldDuration = 0;
                if (!unit.IsNull())
                {
                    (resultOperation, resultDuration, resultOldDuration) = operation switch
                    {
                        BuffOperation.AddBuff => obj.HandleAddOrChangeBuff(unit, buffName, duration, true),
                        BuffOperation.RemoveBuff => obj.HandleRemoveBuff(unit, buffName),
                        BuffOperation.ChangeDuration =>
                            obj.HandleAddOrChangeBuff(unit, buffName, oldDuration + duration, false),
                        BuffOperation.SetDuration => obj.HandleSetDurationBuff(unit, buffName, duration),
                        _ => (BuffOperation.None, duration, 0)
                    };
                }
                r.Operation = resultOperation;
                r.Duration = resultDuration;
                
                if (resultOperation == BuffOperation.RemoveBuff) 
                {
                    var tile = obj.State.TileMapState.GetTile(unit.Coords);
                    foreach (var region in tile.Regions)
                    {
                        var rbs = region.RegionName.GetRegionInfo().RegionBuffs;
                        // 如果结果为移除buff,但是当前所在区域是具有这个buff的区域,则无法移除
                        if (rbs.IndexOf(buffName) == -1) continue;
                        
                        r.UnitId = 0;
                        r.Operation = operation;
                        r.Duration = duration;
                        return r;
                    }
                }

                if (obj.ComplexSimulationRegion(1))
                {
                    switch (resultOperation)
                    {
                        case BuffOperation.AddBuff:
                            NotifyUnitBuff(unit, buffName, b => b.OnAddToUnit(obj, unit, resultDuration));
                            break;
                        case BuffOperation.RemoveBuff:
                            NotifyUnitBuff(unit, buffName, b => b.OnRemoveFromUnit(obj, unit, resultDuration));
                            break;
                        case BuffOperation.ChangeDuration:
                            NotifyUnitBuff(unit, buffName, b => b.OnBuffCountChanged(obj, unit,
                                resultOldDuration + resultDuration,
                                resultOldDuration, resultOldDuration + resultDuration));
                            break;
                    }
                }
                
                return r;
            }
            
            void NotifyUnitBuff(IReadOnlyUnitState unit, string buffName, Action<IBuff> action)
            {
                if (unit.IsNull()) return;
                if (!unit.TryFindBuff(buffName, out var index)) return;
                EnterSimulation();
                obj.PushContext(new SimContext(unit.Id));
                action(unit.Buffs[index].BuffName.GetBuffInfo().Hook);
                obj.PopContext();
                ExitSimulation();
            }
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleAddOrChangeBuff(IReadOnlyUnitState unit, 
            string buffName, int newDuration, bool isAdd)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                var oldDuration = unit.Buffs[index].Count;
                var finalDuration = isAdd ? Mathf.Max(newDuration, oldDuration) : Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                    return (BuffOperation.RemoveBuff, 0, oldDuration);
                }
                
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(index, finalDuration));
                return (BuffOperation.ChangeDuration, finalDuration - oldDuration, oldDuration);
            }
            
            obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffName, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleRemoveBuff(IReadOnlyUnitState unit, 
            string buffName)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                return (BuffOperation.RemoveBuff, 0, 0);
            }
            return (BuffOperation.None, 0, 0);
        }

        private (BuffOperation operation, int duration, int oldDuration) HandleSetDurationBuff(IReadOnlyUnitState unit, 
            string buffName, int newDuration)
        {
            if (unit.TryFindBuff(buffName, out var index))
            {
                var oldDuration = unit.Buffs[index].Count;
                var finalDuration = Mathf.Max(newDuration, 0);
                
                if (finalDuration == 0)
                {
                    obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.RemoveBuff(index));
                    return (BuffOperation.RemoveBuff, 0, oldDuration);
                }
                
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.ModifyBuff(index, finalDuration));
                return (BuffOperation.ChangeDuration, finalDuration - oldDuration, oldDuration);
            }
            
            obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.AddBuff(buffName, newDuration));
            return (BuffOperation.AddBuff, newDuration, 0);
        }
    }
    public static partial void ApplyModifyUnitBuffViewDraw(GameView view, ReadOnlySimObject sim, ApplyModifyUnitBuffResult result)
    {
        if (result.Step.Draw)
        {
            if (result.Operation != BuffOperation.AddBuff && result.Operation != BuffOperation.RemoveBuff) return;
            view.EventsView.BlockDraw.AddBuffChange(result.Step.Coords, result.Step.BuffName,
                result.Operation == BuffOperation.AddBuff, sim.IsFocused);
        }
        var uv = view.UnitsView.GetUnitView(result.UnitId);
        if (uv == null) return;
        uv.BuffView.ChangeBuffChange(result.Step.BuffName, result.Operation == BuffOperation.AddBuff ? 1 : -1);
        view.EventsView.NeedClearUnit(result.UnitId);
    }
    
    public static partial GDTask ApplyModifyUnitBuffViewRun(GameView view, ReadOnlySimObject sim, ApplyModifyUnitBuffResult result)
    {
        if (result.Operation == BuffOperation.AddBuff)
        {
            view.UnitsView.GetUnitView(result.UnitId)?.BuffView
                .AddVisibleBuff(result.Step.BuffName);
        }
        else if (result.Operation == BuffOperation.RemoveBuff)
        {
            view.UnitsView.GetUnitView(result.UnitId)?.BuffView
                .RemoveVisibleBuff(result.Step.BuffName);
        }
        return GDTask.CompletedTask;
    }
}