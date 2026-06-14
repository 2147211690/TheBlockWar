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
        public ApplyCureResult ApplyCure(Vector2I coords, float cure)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;

            ApplyCureResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyCureResult(CurrentDeep, coords, cure, 0, 0));;
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.UnitId = unit.Id;
                if (unit.IsNull() || unit.IsDead)
                {
                    return r;
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
                
                if (unit.IsDead)
                {
                    return r;
                }
                
                var validCure = Mathf.Min(rawCure, unit.MaxHealth - unit.Health);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health += validCure);
                unit = obj.State.UnitMapState.GetUnit(unit.Id);
                r.ValidCure = validCure;

                if (validCure > 0 && obj.ComplexSimulationRegion(0))
                {
                    Hook.BuffUnitAfterGetCure(unit, obj, validCure);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }
                return r;
            }
        }
    }

    public static partial void ApplyCureViewDraw(GameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
        view.EventsView.BlockDraw.AddHealthChange(result.Step.Coords, result.Step.Cure, result.ValidCure, 
                result.UnitId != 0 ,sim.IsFocused);
        var uv = view.UnitsView.GetUnitView(result.UnitId);
        if (uv == null) return;
        uv.HealthBar.HealthChange += result.ValidCure;
        view.EventsView.NeedClearUnit(result.UnitId);
    }

    public static partial GDTask ApplyCureViewRun(GameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
        view.UnitsView.GetUnitView(result.UnitId)?.HealthBar.CurrentHealth += result.ValidCure;
        return GDTask.CompletedTask;
    }
}