using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

public record StepsEventCore(Vector2I? Coords, List<ApplyStep> Steps) : EventCore
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
        obj.ApplySteps(Steps, 0, 0, StepsRunningMode.QueueWaitAll);
    }

    public override bool CheckIsValid(EventState eventState, IReadOnlyGameState gameState)
    {
        return true;
    }
}