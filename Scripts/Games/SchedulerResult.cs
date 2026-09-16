using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games;

public class SchedulerResult
{
    public int RunningCount { get; set; }
    public List<ReadOnlySimObject> Sims { get; set; } = new();
    public IEnumerable<StepResult> Steps => Sims.SelectMany(s => s.Steps);
    public float DelayTime { get; set; }

    public void Replay(IGameState state)
    {
        for (var i = 0; i < RunningCount; i++)
        {
            var sim = Sims[i];
            sim.Steps.Replay(state);
        }
    }
}