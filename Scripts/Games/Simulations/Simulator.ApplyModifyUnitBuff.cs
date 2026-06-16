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
                var r = obj.Steps.AddResult(new ApplyModifyUnitBuffResult(CurrentDeep, coords, buffName, operation,
                    count, draw, forceRemove,0, 0, 0, BuffOperationResult.None));
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                if (unit.IsNull()) return r;
                r.UnitId = unit.Id;
                var index = unit.FindBuff(buffName);
                r.OldCount = index == -1 ? 0 : unit.Buffs[index].Count;
                if (operation == BuffOperation.AddBuff)
                {
                    if (unit.IsDead) return r;
                    var shouldAdd = true;
                    buffName.GetBuffInfo().Hook.OnShouldAddToUnit(obj.State, unit, ref shouldAdd);
                    if (shouldAdd)
                    {
                        var newIndex = 0;
                        obj.State.UnitMapState.ModifyUnit(unit.Id, u => newIndex = u.AddBuff(buffName, count));
                        r.NewCount = newIndex < 0 ? 0 : unit.Buffs[newIndex].Count;
                        if (r.OldCount == 0 && r.NewCount > r.OldCount) r.Result = BuffOperationResult.BuffAdded;
                        else if (r.NewCount != r.OldCount) r.Result = BuffOperationResult.BuffCountChanged;
                    }
                    else
                    {
                        r.Result = BuffOperationResult.None;
                        r.NewCount = r.OldCount;
                    }
                }
                else if (operation == BuffOperation.RemoveBuff && index >= 0)
                {
                    if (unit.IsDead && !forceRemove) return r;
                    var add = true;
                    if (!forceRemove)
                    {
                        var tile = obj.State.TileMapState.GetTile(unit.Coords);
                        foreach (var region in tile.Regions)
                        {
                            var rbs = region.RegionName.GetRegionInfo().RegionBuffs;
                            // 如果结果为移除buff,但是当前所在区域是具有这个buff的区域,则无法移除
                            if (rbs.IndexOf(buffName) == -1) continue;
                            add = false;
                            break;
                        }
                    }

                    if (add)
                    {
                        var newIndex = 0;
                        obj.State.UnitMapState.ModifyUnit(unit.Id, u => newIndex = u.RemoveBuff(buffName, count));
                        r.NewCount = newIndex < 0 ? 0 : unit.Buffs[newIndex].Count;
                        if (r.NewCount == 0 && r.NewCount < r.OldCount) r.Result = BuffOperationResult.BuffRemoved;
                        else if (r.NewCount != r.OldCount) r.Result = BuffOperationResult.BuffCountChanged;
                    }
                    else
                    {
                        r.Result = BuffOperationResult.None;
                        r.NewCount = r.OldCount;
                    }
                }

                if (obj.ComplexSimulationRegion(1))
                {
                    switch (r.Result)
                    {
                        case BuffOperationResult.BuffAdded:
                            NotifyUnitBuff(unit, buffName, b => b.OnAddToUnit(obj, unit, r.NewCount));
                            break;
                        case BuffOperationResult.BuffRemoved:
                            NotifyUnitBuff(unit, buffName, b => b.OnRemoveFromUnit(obj, unit, r.NewCount, r.OldCount));
                            break;
                        case BuffOperationResult.BuffCountChanged:
                            NotifyUnitBuff(unit, buffName, b => b.OnBuffCountChanged(obj, unit,
                                r.NewCount,
                                r.OldCount,
                                r.NewCount));
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

    public static partial void ApplyModifyUnitBuffViewDraw(GameView view, ReadOnlySimObject sim,
        ApplyModifyUnitBuffResult result)
    {
        if (result.Step.Draw)
        {
            view.EventsView.BlockDraw.AddBuffChange(result.Step.Coords, result.Step.BuffName,
                result.Step.Operation == BuffOperation.AddBuff, sim.IsFocused);
        }

        var uv = view.UnitsView.GetUnitView(result.UnitId);
        if (uv == null) return;
        uv.BuffView.ChangeBuffChange(result.Step.BuffName, Mathf.Sign(result.NewCount - result.OldCount));
        view.EventsView.NeedClearUnit(result.UnitId);
    }

    public static partial GDTask ApplyModifyUnitBuffViewRun(GameView view, ReadOnlySimObject sim,
        ApplyModifyUnitBuffResult result)
    {
        if (result.Result == BuffOperationResult.BuffAdded)
        {
            view.UnitsView.GetUnitView(result.UnitId)?.BuffView
                .AddVisibleBuff(result.Step.BuffName);
        }
        else if (result.Result == BuffOperationResult.BuffRemoved)
        {
            view.UnitsView.GetUnitView(result.UnitId)?.BuffView
                .RemoveVisibleBuff(result.Step.BuffName);
        }

        return GDTask.CompletedTask;
    }
}