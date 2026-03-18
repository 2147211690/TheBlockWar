using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyPushUnit(Vector2I coords, DirMask dir, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, 0, true));
                    return;
                }

                var unit = state.UnitMapState.GetUnit(coords);
                var target = coords + dir.ToVec2I();
                var targetUnit = state.UnitMapState.GetUnit(target);
                var validPush = !unit.IsNull() && targetUnit.IsNull() && state.IsInMap(target);
                result.Steps?.AddStep(new PushUnitStep(IsCurrentChildSimulation, coords, dir, unit.Id, validPush));
                if (unit.IsNull()) return;
                if (validPush)
                {
                    state.ApplyMoveUnit(coords, target, result);
                }
                else
                {
                    state.ApplyDamage(coords, 10, result);
                    if (!targetUnit.IsNull()) state.ApplyDamage(target, 10, result);
                }
            }
        }
    }
}