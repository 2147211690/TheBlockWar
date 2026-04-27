using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyMoveUnit(Vector2I coords, Vector2I to)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                bool isValid = true;
                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, 0, isValid));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                if (!unit.IsNull())
                {
                    obj.State.UnitMapState.ModifyUnit(unit.Id, u => u.Coords = to);
                    obj.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, unit.Id, isValid));
                    obj.ApplyUnitRemoveFromCoords(unit, coords);
                    obj.ApplyUnitSetInCoords(unit);
                }
                else obj.Steps?.AddStep(new MoveUnitStep(CurrentDeep, coords, to, unit.Id, isValid));
            }
        }
    }
}