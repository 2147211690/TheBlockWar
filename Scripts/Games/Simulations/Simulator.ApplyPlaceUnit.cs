using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitState:UnitState Valid:bool
        [Step(["UnitState", "Valid"], [typeof(UnitState), typeof(bool)])]
        public ApplyPlaceUnitResult ApplyPlaceUnit(Vector2I coords, string unitName, int unitTeam)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyPlaceUnitResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyPlaceUnitResult(
                    CurrentDeep, CurrentTeam, coords, unitName, unitTeam, UnitState.Null, false));
                r.ReValid = !obj.State.UnitMapState.HasUnit(coords);
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }
                
                if (obj.ComplexSimulationRegion( 2))
                {
                    //如果在等待层,或者此处有单位,则放置失败
                    if (!r.ReValid)
                    {
                        return r;
                    }
                    var unitState = UnitFactory.CreateUnit(unitName, unitTeam, obj.State.IdGenerator.NewLong(), coords);
                    obj.State.UnitMapState.AddUnit(unitState);
                    r.ReUnitState = unitState.Clone();
                    //单位buff初始化
                    var info = unitName.GetUnitInfo();
                    var newUnit = obj.State.UnitMapState.GetUnit(unitState.Id);
                    if (info.Buffs is { Length: > 0 })
                    {
                        obj.State.UnitMapState.ModifyUnit(newUnit.Id, u =>
                        {
                            for (var i = 0; i < info.Buffs.Length; i++)
                            {
                                var buffName = info.Buffs[i];
                                u.AddBuff(buffName, info.BuffCounts[i]);
                            }
                        });
                        Hook.BuffAddToUnit(newUnit, obj);
                    }
                    //注册光环buff
                    foreach (var auraBuff in obj.State.BuffGlobalState.GetTargetBuffs(newUnit.Name, newUnit.Team))
                    {
                        obj.ApplyModifyUnitBuff(newUnit.Coords, auraBuff, BuffOperation.AddBuff, 1, false);
                    }
                    //应用单位放置到坐标的额外操作
                    obj.ApplyUnitSetInCoords(obj.State.UnitMapState.GetUnit(unitState.Id));
                }
                return r;
            }
        }
    }
    public static partial void ApplyPlaceUnitReplayCore(IGameState state, ApplyPlaceUnitResult result)
    {
        if (!result.ReValid) return;
        var unitState = result.ReUnitState.Clone();
        state.UnitMapState.AddUnit(unitState);
        var info = result.UnitName.GetUnitInfo();
        var newUnit = state.UnitMapState.GetUnit(result.ReUnitState.Id);
        if (info.Buffs is { Length: > 0 })
        {
            state.UnitMapState.ModifyUnit(newUnit.Id, u =>
            {
                for (var i = 0; i < info.Buffs.Length; i++)
                {
                    var buffName = info.Buffs[i];
                    u.AddBuff(buffName, info.BuffCounts[i]);
                }
            });
        }
    }
    public static partial void ApplyPlaceUnitViewDraw(IGameView view, ReadOnlySimObject sim, ApplyPlaceUnitResult result)
    {
    }
    
    public static partial GDTask ApplyPlaceUnitViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyPlaceUnitResult result)
    {
        if (result.ReValid)
        {
            var uv = view.CreateUnitView(
                result.Coords,
                result.ReUnitState);
            if (uv == null) return GDTask.CompletedTask;
            //初始化buff视图
            foreach (var a in result.UnitName.GetUnitInfo().Buffs)
                uv.BuffView.AddVisibleBuff(a);
            return AnimationHelper.DropInHeavyAsync(uv, 35);
        }
        return GDTask.CompletedTask;
    }
}