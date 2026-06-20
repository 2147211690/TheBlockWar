using System.Linq;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class SchedulerResult
{
    public int RunningCount { get; set; }
    public List<ReadOnlySimObject> Sims { get; } = new();
    public IEnumerable<StepResult> Steps => Sims.SelectMany(s => s.Steps);
}