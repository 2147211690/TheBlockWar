using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;
[RegisterChildClass(typeof(Regions))]
public abstract class Region(RegionNameEnum name) : IRegion
{
    protected Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public RegionNameEnum Name { get; } = name;

    public virtual void OnUnitEnter(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationContext context)
    {
    }

    public virtual void OnUnitLeave(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationContext context)
    {
    }

    public virtual void OnEnterTurn(IGameState state, Vector2I regionCoords, int turn, SimulationContext context)
    {
    }

    public virtual void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context)
    {
        
    }
}