using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

public class Region : IRegion
{
    protected Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public Region(string infoPath)
    {
        Info = ResourceLoader.Exists(infoPath) ? ResourceLoader.Load<RegionInfo>(infoPath) : RegionInfo.Null;
        Logger.Warn("RegionInfo-{path}不存在,使用空替代", infoPath);
    }
    public RegionInfo Info { get; }

    public virtual void OnUnitEnter(IGameState state, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationResult result)
    {
    }

    public virtual void OnUnitLeave(IGameState state, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationResult result)
    {
    }

}