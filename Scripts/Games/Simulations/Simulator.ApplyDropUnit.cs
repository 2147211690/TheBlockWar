using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        //result:UnitId:long Valid:bool
        public void ApplyDropUnit(Vector2I coords)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {

                if (obj.Layer == SimulationLayer.Waiting)
                {
                    obj.Steps?.AddStep(new DropUnitStep(CurrentDeep, coords, 0, false));
                    return;
                }

                var unit = obj.State.UnitMapState.GetUnit(coords);
                obj.State.UnitMapState.RemoveUnit(unit.Id);
                obj.Steps?.AddStep(new DropUnitStep(CurrentDeep, coords, unit.Id, true));
            }
        }
    }
}