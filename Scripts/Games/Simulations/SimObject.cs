using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimObject(IGameState state, SimulationLayer layer, SimStepList? steps, long sourceEventId)
{
    public IGameState State { get; set; } = state;
    public SimulationLayer Layer { get; set; } = layer;
    public SimStepList? Steps { get; set; } = steps;
    public long SourceEventId { get; set; } = sourceEventId;
}
public enum SimulationLayer
{
    Running,//进行实际的模拟
    Focused,//进行复杂的模拟
    Waiting,//不进行复杂的模拟
    Disabled,//已禁用
}