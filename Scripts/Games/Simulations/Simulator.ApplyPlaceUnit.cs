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
        public void ApplyPlaceUnit(Vector2I coords, string unitName, int team)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = !obj.State.UnitMapState.HasUnit(coords);
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyPlaceUnitResult(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                    return;
                }
                
                if (obj.ComplexSimulationRegion( 2))
                {
                    //如果在等待层,或者此处有单位,则放置失败
                    if (!isValid)
                    {
                        obj.Steps?.AddResult(new ApplyPlaceUnitResult(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                        return;
                    }
                    var unitState = UnitFactory.CreateUnit(unitName, team, obj.State.IdGenerator.NewLong(), coords);
                    obj.State.UnitMapState.AddUnit(unitState);
                    obj.Steps?.AddResult(new ApplyPlaceUnitResult(CurrentDeep, coords, unitName, team, unitState.Clone(),
                        isValid));
                    //单位buff初始化
                    var info = unitName.GetUnitInfo();
                    var newUnit = obj.State.UnitMapState.GetUnit(unitState.Id);
                    if (info.Buffs is { Length: > 0 })
                    {
                        obj.State.UnitMapState.ModifyUnit(newUnit.Id, u =>
                        {
                            foreach (var buffName in info.Buffs) u.AddBuff(buffName, 1);
                        });
                        Hook.BuffAddToUnit(newUnit, obj);
                    }
                    //应用单位放置到坐标的额外操作
                    obj.ApplyUnitSetInCoords(obj.State.UnitMapState.GetUnit(unitState.Id));
                }
                else
                {
                    obj.Steps?.AddResult(new ApplyPlaceUnitResult(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                }
            }
        }
    }
    public static partial void ApplyPlaceUnitViewDraw(GameView view, ReadOnlySimObject sim, ApplyPlaceUnitResult result)
    {
    }
    
    public static partial GDTask ApplyPlaceUnitViewRun(GameView view, ReadOnlySimObject sim, ApplyPlaceUnitResult result)
    {
        return result.Valid
            ? view.UnitsView.PlaceUnit(
                result.Step.Coords,
                result.UnitState,
                view.GetTeamType(result.Step.Team))
            : GDTask.CompletedTask;
    }
}