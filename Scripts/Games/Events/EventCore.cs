using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public abstract record EventCore : IEventCore
{
    public abstract long GetBindUnitId(EventState eventState);
    public virtual Vector2I? GetCoords(EventState eventState) => null;
    public abstract void OnApply(EventState eventState, SimObject obj);
    public abstract bool CheckIsValid(EventState eventState, IReadOnlyGameState gameState);
}