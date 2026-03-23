using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Regions;

public class Region : IRegion
{
    protected Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public Region(string infoPath)
    {
        Info = ResourceLoader.Exists(infoPath) ? ResourceLoader.Load<RegionInfo>(infoPath) : RegionInfo.Null;
        Logger.Warn("RegionInfo-{path}不存在,使用空替代", infoPath);
    }
    public RegionInfo Info { get; }

    public virtual void OnUnitEnter(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationResult result)
    {
    }

    public virtual void OnUnitLeave(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationResult result)
    {
    }

    public virtual void OnEnterTurn(IGameState state, Vector2I regionCoords, int turn, SimulationResult result)
    {
    }

    public virtual void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationResult result)
    {
        
    }
}