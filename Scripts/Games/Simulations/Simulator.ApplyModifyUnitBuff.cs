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
        [Step(["UnitId", "OldCount", "NewCount", "Result"],
            [typeof(long), typeof(int), typeof(int), typeof(BuffOperationResult)])]
        public ApplyModifyUnitBuffResult ApplyModifyUnitBuff(Vector2I coords, string buffName, BuffOperation operation,
            int count, bool draw = true, bool forceRemove = false)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyModifyUnitBuffResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyModifyUnitBuffResult(
                    CurrentDeep, CurrentTeam, coords, buffName, operation,
                    count, draw, forceRemove,0, 0, 0, BuffOperationResult.None));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                if (unit.IsNull()) return r;
                r.ReUnitId = unit.Id;
                var index = unit.FindBuff(buffName);
                r.ReOldCount = index == -1 ? 0 : unit.Buffs[index].Count;
                if (operation == BuffOperation.AddBuff)
                {
                    if (unit.IsDead) return r;
                    var shouldAdd = true;
                    buffName.GetBuffInfo().Hook.OnShouldAddToUnit(obj.State, unit, ref shouldAdd);
                    if (shouldAdd)
                    {
                        var newIndex = 0;
                        obj.State.UnitMapState.ModifyUnit(unit.Id, u => newIndex = u.AddBuff(buffName, count));
                        r.ReNewCount = newIndex < 0 ? 0 : unit.Buffs[newIndex].Count;
                        if (r.ReOldCount == 0 && r.ReNewCount > r.ReOldCount) r.ReResult = BuffOperationResult.BuffAdded;
                        else if (r.ReNewCount != r.ReOldCount) r.ReResult = BuffOperationResult.BuffCountChanged;
                    }
                    else
                    {
                        r.ReResult = BuffOperationResult.None;
                        r.ReNewCount = r.ReOldCount;
                    }
                }
                else if (operation == BuffOperation.RemoveBuff && index >= 0)
                {
                    if (unit.IsDead && !forceRemove) return r;
                    var shouldRemove = true;
                    if (!forceRemove)
                    {
                        var tile = obj.State.TileMapState.GetTile(unit.Coords);
                        foreach (var region in tile.Regions)
                        {
                            var rbs = region.RegionName.GetRegionInfo().RegionBuffs;
                            // 如果结果为移除buff,但是当前所在区域是具有这个buff的区域,则无法移除
                            if (Array.FindIndex(rbs, b => b.BuffName == buffName) == -1) continue;
                            shouldRemove = false;
                            break;
                        }
                    }

                    if (shouldRemove)
                    {
                        var newIndex = 0;
                        obj.State.UnitMapState.ModifyUnit(unit.Id, u => newIndex = u.RemoveBuff(buffName, count));
                        r.ReNewCount = newIndex < 0 ? 0 : unit.Buffs[newIndex].Count;
                        if (r.ReNewCount == 0 && r.ReNewCount < r.ReOldCount) r.ReResult = BuffOperationResult.BuffRemoved;
                        else if (r.ReNewCount != r.ReOldCount) r.ReResult = BuffOperationResult.BuffCountChanged;
                    }
                    else
                    {
                        r.ReResult = BuffOperationResult.None;
                        r.ReNewCount = r.ReOldCount;
                    }
                }
                
                if (obj.ComplexSimulationRegion(3))
                {
                    switch (r.ReResult)
                    {
                        case BuffOperationResult.BuffAdded:
                            NotifyUnitBuff(unit, buffName, b => b.OnAddToUnit(obj, unit, r.ReNewCount));
                            break;
                        case BuffOperationResult.BuffRemoved:
                            NotifyUnitBuff(unit, buffName, b => b.OnRemoveFromUnit(obj, unit, r.ReNewCount, r.ReOldCount));
                            break;
                        case BuffOperationResult.BuffCountChanged:
                            NotifyUnitBuff(unit, buffName, b => b.OnBuffCountChanged(obj, unit,
                                r.ReNewCount,
                                r.ReOldCount,
                                r.ReNewCount));
                            break;
                    }
                }

                return r;
            }

            void NotifyUnitBuff(IReadOnlyUnitState unit, string buffName, Action<IBuff> action)
            {
                if (unit.IsNull() || unit.IsDead) return;
                if (!unit.TryFindBuff(buffName, out var index)) return;
                EnterSimulation();
                obj.PushContext(new SimContext(unit.Id));
                action(unit.Buffs[index].BuffName.GetBuffInfo().Hook);
                obj.PopContext();
                ExitSimulation();
            }
        }
    }
    
    public static partial void ApplyModifyUnitBuffReplayCore(IGameState state, ApplyModifyUnitBuffResult result)
    {
        state.UnitMapState.ModifyUnit(result.ReUnitId, u =>
        {
            switch (result.ReResult)
            {
                case BuffOperationResult.None:
                    break;
                case not BuffOperationResult.None:
                    var change = result.ReNewCount - result.ReOldCount;
                    if (change > 0)
                    {
                        u.AddBuff(result.BuffName, change);
                    }
                    else if (change < 0)
                    {
                        u.RemoveBuff(result.BuffName, -change);
                    }
                    break;
            }
        });
    }
    
    public static partial void ApplyModifyUnitBuffViewDraw(IGameView view, ReadOnlySimObject sim,
        ApplyModifyUnitBuffResult result)
    {
        if (result.Draw)
        {
            view.EventBuff(result.Coords, result.BuffName,
                result.ReNewCount - result.ReOldCount, sim.IsFocused);
        }

        var uv = view.EventUnit(result.ReUnitId);
        if (uv == null) return;
        uv.BuffView.EventBuffChange(result.BuffName, Mathf.Sign(result.ReNewCount - result.ReOldCount));
    }

    public static partial GDTask ApplyModifyUnitBuffViewRunAsync(IGameView view, ReadOnlySimObject sim,
        ApplyModifyUnitBuffResult result)
    {
        if (result.ReResult == BuffOperationResult.BuffAdded)
        {
            view.GetUnitView(result.ReUnitId)?.BuffView
                .AddVisibleBuff(result.BuffName);
        }
        else if (result.ReResult == BuffOperationResult.BuffRemoved)
        {
            view.GetUnitView(result.ReUnitId)?.BuffView
                .RemoveVisibleBuff(result.BuffName);
        }

        return GDTask.CompletedTask;
    }
}