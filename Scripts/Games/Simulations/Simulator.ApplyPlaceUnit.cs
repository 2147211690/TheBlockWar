using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Units;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitState:UnitState Valid:bool
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
                    obj.Steps?.AddStep(new PlaceUnitStep(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                    return;
                }
                
                if (obj.ComplexSimulationRegion( 2))
                {
                    //如果在等待层,或者此处有单位,则放置失败
                    if (!isValid)
                    {
                        obj.Steps?.AddStep(new PlaceUnitStep(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                        return;
                    }
                    var unitState = UnitFactory.CreateUnit(unitName, team, obj.State.IdGenerator.NewLong(), coords);
                    obj.State.UnitMapState.AddUnit(unitState);
                    obj.Steps?.AddStep(new PlaceUnitStep(CurrentDeep, coords, unitName, team, unitState,
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
                    obj.Steps?.AddStep(new PlaceUnitStep(CurrentDeep, coords, unitName, team, UnitState.Null, isValid));
                }
            }
        }
    }
}