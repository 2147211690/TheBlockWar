using System.Collections.Generic;

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
    public List<SimStep> Steps { get; set; } = new();
    public SimStep PeekStep => Steps.Count > 0 ? Steps[^1] : SimStep.Default;
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