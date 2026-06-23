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
                var r = obj.Steps.AddResult(new ApplyCureResult(CurrentDeep, CurrentTeam, coords, cure, 0, 0));;
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    return r;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                r.ReUnitId = unit.Id;
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
                r.ReValidCure = validCure;

                if (validCure > 0 && obj.ComplexSimulationRegion(0))
                {
                    Hook.BuffUnitAfterGetCure(unit, obj, validCure);
                    unit = obj.State.UnitMapState.GetUnit(unit.Id);
                }
                return r;
            }
        }
    }

    public static partial void ApplyCureReplayCore(IGameState state, ApplyCureResult result)
    {
        state.UnitMapState.ModifyUnit(result.ReUnitId, u => u.Health += result.ReValidCure);
    }

    public static partial void ApplyCureViewDraw(IGameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
        view.EventHealth(result.Coords, result.Cure, result.ReValidCure, 
                result.ReUnitId != 0 ,sim.IsFocused);
        
        var uv = view.EventUnit(result.ReUnitId);
        if (uv == null) return;
        uv.HealthBar.EventHealthChange += result.ReValidCure;
    }

    public static partial GDTask ApplyCureViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyCureResult result)
    {
        view.GetUnitView(result.ReUnitId)?.HealthBar.CurrentHealth += result.ReValidCure;
        return GDTask.CompletedTask;
    }
}