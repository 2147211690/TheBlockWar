using System.Collections.Generic;
using System.Linq;

namespace TheBlockWar.Scripts.Games;

public class SimulationResult
{
    public SimulationResult(uint sourceId)
    {
        SourceId = sourceId;
    }
    public uint SourceId { get; }
    public TeamTypeEnum TeamType { get; set; } = TeamTypeEnum.Player;
    public SimulationLayer Layer { get; set; } = SimulationLayer.Waiting;
    public IEnumerable<SimStep> MasterSteps => Steps.Where(a => !a.IsChildStep);
    public IReadOnlyList<SimStep> Steps => _steps;
    private readonly List<SimStep> _steps = [];
    public SimStep? PeekStep { get; private set; }
    
    public void AddStep(SimStep step)
    {
        _steps.Add(step);
        if (!step.IsChildStep) PeekStep = step;
    }

    public override string ToString()
    {
        return $"结果来源于事件:{SourceId} 层:{Layer} 步骤数:{Steps.Count}\n详细步骤:\n{string.Join("\n", Steps)}";
    }
}

public enum SimulationLayer
{
    Running,//进行实际的模拟
    Focused,//进行复杂的模拟
    Waiting,//不进行复杂的模拟
}