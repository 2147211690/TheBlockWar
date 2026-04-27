using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Regions;
[RegisterChildClass(typeof(Regions))]
public abstract class Region(string name) : IRegion
{
    protected Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    public string Name => name;

    public virtual void OnUnitEnter(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I enterCoords, bool isAddRegion, SimulationContext context)
    {
        var info = Name.GetRegionInfo();
        if (info.EnterAddBuffs != null)
        {
            foreach (var buff in info.EnterAddBuffs)
                state.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.AddBuff, int.MaxValue, context);
        }
    }

    public virtual void OnUnitLeave(IGameState state, Vector2I regionCoords, IReadOnlyUnitState unit, Vector2I leaveCoords, bool isRemoveRegion, SimulationContext context)
    {
        var info = Name.GetRegionInfo();
        if (info.ExitRemoveBuffs != null)
        {
            foreach (var buff in info.ExitRemoveBuffs)
                state.ApplyModifyUnitBuff(unit.Coords, buff, BuffOperation.RemoveBuff, int.MaxValue, context);
        }
    }

    public virtual void OnEnterTurn(IGameState state, Vector2I regionCoords, int turn, SimulationContext context)
    {
    }
}