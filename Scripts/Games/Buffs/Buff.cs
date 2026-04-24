using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Games.Buffs;

public class Buff : IBuff
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public Buff(string infoPath)
    {
        Info = ResourceLoader.Exists(infoPath) ? ResourceLoader.Load<BuffInfo>(infoPath) : BuffInfo.Null;
        //Logger.Warn("BuffInfo-{path}不存在,使用空替代", infoPath);
    }

    public BuffInfo Info { get; }

    public virtual void OnAddToUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
    }

    public virtual void OnUnitBeforeDamage(IGameState state, IReadOnlyUnitState unit, float rawDamage,
        ref float resultDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterDamage(IGameState state, IReadOnlyUnitState unit, float getDamage,
        SimulationContext context)
    {
    }

    public virtual void OnUnitBeforeCure(IGameState state, IReadOnlyUnitState unit, float rawCure, ref float resultCure,
        SimulationContext context)
    {
    }

    public virtual void OnUnitAfterCure(IGameState state, IReadOnlyUnitState unit, float getCure,
        SimulationContext context)
    {
    }

    public virtual void OnRemoveFromUnit(IGameState state, IReadOnlyUnitState unit, SimulationContext context)
    {
    }

    public virtual void OnDurationChanged(IGameState state, IReadOnlyUnitState unit, int oldDuration, int newDuration,
        SimulationContext context)
    {
    }

    public virtual void OnEnterTurn(IGameState state, IReadOnlyUnitState unit, int turn, SimulationContext context)
    {
    }
}