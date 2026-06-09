using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class SchedulerResult
{
    public required int Team { get; set; }
    public required TeamTypeEnum TeamType { get; set; }
    public required SimResultList Results { get; set; }
}