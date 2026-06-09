using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long ValidCure:float
        [Step(["UnitId", "ValidCure"],[typeof(long), typeof(float)])]
        public void ApplyCure(Vector2I coords, float cure)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyCureResult(CurrentDeep, coords, cure, 0, 0));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                float validCure = 0;
                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddResult(
                        new ApplyCureResult(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                var originCure = cure;
                var rawCure = cure;

                if (obj.ComplexSimulationRegion(0))
                {
                    //应用来源单位治疗前事件
                    var source = obj.State.UnitMapState.GetUnit(CurrentContext.SourceUnitId);
                    Hook.BuffBeforeCureToUnit(source, obj, unit, originCure, ref rawCure);
                    //应用单位治疗前事件
                    var origin = rawCure;
                    Hook.BuffUnitBeforeGetCure(unit, obj, origin, ref rawCure);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }

                if (unit.IsNull())
                {
                    obj.Steps?.AddResult(
                        new ApplyCureResult(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                validCure = Mathf.Min(rawCure, unit.MaxHealth - unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health += validCure);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);

                if (unit.IsNull())
                {
                    obj.Steps?.AddResult(
                        new ApplyCureResult(CurrentDeep, coords, cure, unit.Id, validCure));
                    return;
                }

                obj.Steps?.AddResult(new ApplyCureResult(CurrentDeep, coords, cure, unit.Id, validCure));

                if (validCure > 0 && obj.ComplexSimulationRegion(0))
                {
                    Hook.BuffUnitAfterGetCure(unit, obj, validCure);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }
            }
        }
    }

    public static partial void ApplyCureViewDraw(GameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
    }

    public static partial GDTask ApplyCureViewRun(GameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
        view.UnitsView.GetUnitView(result.UnitId)?.HealthBar.CurrentHealth += result.ValidCure;
        return GDTask.CompletedTask;
    }
}