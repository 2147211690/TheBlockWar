using System.Collections.Generic;

namespace TheBlockWar.Scripts.Games;

public class SimulationResult
{
    public SimulationResult(uint sourceId)
    {
        SourceId = sourceId;
    }
    public uint SourceId { get; }
    public SimulationLayer Layer { get; set; } = SimulationLayer.Waiting;
    public List<SimStep> Steps { get; set; } = new();
}

public enum SimulationLayer
{
    Running,//进行实际的模拟
    Focused,//进行复杂的模拟
    Waiting,//不进行复杂的模拟
}