using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Regions;
[RegisterChildClass]
public abstract class Region(string name) : IRegion
{
    protected Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Name => name;

    public virtual void OnUnitEnter(SimObject obj, Vector2I regionCoords, string regionData, IReadOnlyUnitState unit, Vector2I enterCoords,
        bool isAddRegion)
    {
    }

    public virtual void OnUnitLeave(SimObject obj, Vector2I regionCoords, string regionData, IReadOnlyUnitState unit, Vector2I leaveCoords,
        bool isRemoveRegion)
    {
    }

    public virtual void OnEnterTurn(SimObject obj, Vector2I regionCoords, string regionData, int turn)
    {
    }

    public virtual void OnAddToTile(SimObject obj, Vector2I regionCoords, string regionData)
    {
    }

    public virtual void OnRemoveFromTile(SimObject obj, Vector2I regionCoords, string regionData)
    {
    }

    public virtual void OnCreateRegionEvent(SimObject obj, Vector2I regionCoords, string regionData, ref IEventCore eventCore)
    {
    }
}