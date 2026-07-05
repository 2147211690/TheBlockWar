using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public record ActionEventCore(Vector2I? Coords, Action<EventState, SimObject> ApplyAction, Func<EventState, bool>? CheckIsValidFunc = null) : EventCore
{
    public override Vector2I? GetCoords(EventState eventState)
    {
        return Coords;
    }
    public override long GetBindUnitId(EventState eventState)
    {
        return 0;
    }

    public override void OnApply(EventState eventState, SimObject obj)
    {
        ApplyAction(eventState, obj);
    }

    public override bool CheckIsValid(EventState eventState, IReadOnlyGameState gameState)
    {
        return CheckIsValidFunc?.Invoke(eventState) ?? true;
    }
}