using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        public void ApplyPlaceUnit(Vector2I coords, UnitState unitState, SimulationResult result)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = state.UnitMapState.GetUnit(coords).IsNull();
                if (result.Layer == SimulationLayer.Waiting)
                {
                    result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, null, 0, isValid));
                    return;
                }

                //如果在等待层,或者此处有单位,则放置失败
                if (!isValid)
                {
                    result.Steps?.AddStep(
                        new PlaceUnitStep(IsCurrentChildSimulation, coords, null, unitState.Id, false));
                    return;
                }

                unitState.Coords = coords;
                state.UnitMapState.AddUnit(unitState);
                result.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitState, unitState.Id,
                    isValid));
                //应用单位放置到坐标的额外操作
                state.ApplyUnitSetInCoords(state.UnitMapState.GetUnit(unitState.Id), result);
            }
        }
    }
}