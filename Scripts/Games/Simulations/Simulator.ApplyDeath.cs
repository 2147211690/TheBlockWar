using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyDeath(Vector2I coords, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, 0, false));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);

                if (unit.IsNull() || unit.IsDead)
                {
                    context.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit.Id, false));
                    return;
                }

                context.Steps?.AddStep(new DeathStep(IsCurrentChildSimulation, coords, unit.Id, true));
                state.UnitMapState.ModifyUnit(unit.Id, u => u.IsDead = true);
                //TODO:可能引入区域相关事件
            }
        }
    }
}