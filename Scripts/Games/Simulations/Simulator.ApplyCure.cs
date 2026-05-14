using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long ValidCure:float
        public void ApplyCure(Vector2I coords, float cure)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new CureStep(CurrentDeep, coords, cure, 0, 0));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                float validCure = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                var originCure = cure;
                var rawCure = cure;

                if (obj.ComplexSimulationRegion(0))
                {
                    //应用来源单位治疗前事件
                    var source = obj.State.UnitMapState.GetUnit(CurrentContext.SourceUnitId);
                    obj.NotifyUnitBuffs(source,
                        b => b.OnBeforeCureToUnit(obj, source, unit, originCure, ref rawCure));
                    //应用单位治疗前事件
                    obj.NotifyUnitBuffs(unit,
                        b => b.OnUnitBeforeGetCure(obj, unit, rawCure, ref rawCure));
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    obj.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                validCure = Mathf.Min(rawCure, unit.MaxHealth - unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health += validCure);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    obj.Steps?.AddStep(
                        new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                obj.Steps?.AddStep(new CureStep(CurrentDeep, coords, cure, unit.Id, validCure));

                if (validCure > 0 && obj.ComplexSimulationRegion(0))
                {
                    obj.NotifyUnitBuffs(unit, b => b.OnUnitAfterGetCure(obj, unit, validCure));
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }
            }
        }
    }
}