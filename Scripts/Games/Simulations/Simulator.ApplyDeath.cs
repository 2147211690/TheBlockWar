using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        [Step(["UnitId", "Valid"], [typeof(long), typeof(bool)])]
        public void ApplyDeath(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddResult(new ApplyDeathResult(CurrentDeep, coords, 0, false));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);

                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddResult(new ApplyDeathResult(CurrentDeep, coords, unit.Id, false));
                    return;
                }

                obj.Steps?.AddResult(new ApplyDeathResult(CurrentDeep, coords, unit.Id, true));
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Health = 0);
                Hook.BuffUnitDeath(unit, obj);
                //TODO:可能引入区域相关事件
            }
        }
    }
    public static partial void ApplyDeathViewDraw(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
    }
    
    public static partial GDTask ApplyDeathViewRun(GameView view, ReadOnlySimObject sim, ApplyDeathResult result)
    {
        view.UnitsView.GetUnitView(result.UnitId)?.HealthBar.CurrentHealth = 0;
        return GDTask.CompletedTask;
    }
}