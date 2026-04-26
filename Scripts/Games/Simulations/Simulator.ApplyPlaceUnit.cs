using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(IGameState state)
    {
        //TODO:prefabUnit改为枚举
        public void ApplyPlaceUnit(Vector2I coords, string unitName, int team, UnitState? prefabUnit, SimulationContext context)
        {
            EnterSimulation();
            Main();
            ExitSimulation();

            void Main()
            {
                bool isValid = !state.UnitMapState.HasUnit(coords);
                if (context.Layer == SimulationLayer.Waiting)
                {
                    context.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitName, team, UnitState.Null, isValid));
                    return;
                }
                
                if (ComplexSimulationRegion(context, 2))
                {
                    //如果在等待层,或者此处有单位,则放置失败
                    if (!isValid)
                    {
                        context.Steps?.AddStep(
                            new PlaceUnitStep(IsCurrentChildSimulation, coords, unitName, team, UnitState.Null, isValid));
                        return;
                    }
                    var unitState = prefabUnit is null || prefabUnit.IsNull() ? 
                        UnitFactory.CreateUnit(unitName, team, state.IdGenerator.NewLong(), coords) : prefabUnit;
                    state.UnitMapState.AddUnit(unitState);
                    context.Steps?.AddStep(new PlaceUnitStep(IsCurrentChildSimulation, coords, unitName, team, unitState,
                        isValid));
                    //应用单位放置到坐标的额外操作
                    state.ApplyUnitSetInCoords(state.UnitMapState.GetUnit(unitState.Id), context);
                }
                else
                {
                    context.Steps?.AddStep(
                        new PlaceUnitStep(IsCurrentChildSimulation, coords, unitName, team, UnitState.Null, isValid));
                }
            }
        }
    }
}