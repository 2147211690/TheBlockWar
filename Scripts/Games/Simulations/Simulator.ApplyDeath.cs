using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyDeath(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new DeathStep(CurrentDeep, coords, 0, false));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);

                if (unit.IsNull() || unit.IsDead)
                {
                    obj.Steps?.AddStep(new DeathStep(CurrentDeep, coords, unit.Id, false));
                    return;
                }

                obj.Steps?.AddStep(new DeathStep(CurrentDeep, coords, unit.Id, true));
                obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
                obj.NotifyUnitBuffs(unit, b => b.OnUnitDeath(obj, unit));
                //TODO:可能引入区域相关事件
            }
        }
    }
}