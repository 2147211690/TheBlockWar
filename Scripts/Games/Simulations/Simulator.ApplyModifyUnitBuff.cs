using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Operation:BuffOperation Duration:int
        public void ApplyModifyUnitBuff(Vector2I coords, string buffName, BuffOperation operation, 
            int duration)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            
            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new ModifyUnitBuffStep(CurrentDeep, coords, buffName, operation,
                        duration, 0, operation, duration));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                var oldDuration = unit.GetBuffDuration(buffName);
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

                if (resultOperation == BuffOperation.RemoveBuff) 
                {
                    var tile = obj.State.TileMapState.GetTile(unit.Coords);
                    foreach (var region in tile.GetInfoAndCurrentRegions())
                    {
                        var rbs = region.GetRegionInfo().RegionBuffs;
                        // 如果结果为移除buff,但是当前所在区域是具有这个buff的区域,则无法移除
                        if (rbs.IndexOf(buffName) == -1) continue;
                        
                        obj.Steps?.AddStep(new ModifyUnitBuffStep(CurrentDeep, coords, buffName, operation,
                            duration, 0, operation, duration));
                        return;
                    }
                }

                obj.Steps?.AddStep(new ModifyUnitBuffStep(CurrentDeep, coords, buffName, operation,
                    duration, unit.Id, resultOperation, resultDuration));

                if (obj.ComplexSimulationRegion(1))
                {
                    switch (resultOperation)
                    {
                        case BuffOperation.AddBuff:
                            obj.NotifyUnitBuffs(unit, b => b.OnAddToUnit(obj, unit));
                            break;
                        case BuffOperation.RemoveBuff:
                            NotifyUnitBuff(unit, buffName, b => b.OnRemoveFromUnit(obj, unit));
                            break;
                        case BuffOperation.ChangeDuration:
                            NotifyUnitBuff(unit, buffName, b => b.OnDurationChanged(obj, unit,
                                resultOldDuration, resultOldDuration + resultDuration));
                            break;
                    }
                }
            }
            
            void NotifyUnitBuff(IReadOnlyUnitState unit, string buffName, Action<IBuff> action)
            {
                if (unit.IsNull()) return;
                if (!unit.TryFindBuff(buffName, out var index)) return;
                EnterSimulation();
                action(unit.Buffs[index].BuffName.GetBuffInstance());
                ExitSimulation();
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
                var oldDuration = unit.Buffs[index].Duration;
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
}