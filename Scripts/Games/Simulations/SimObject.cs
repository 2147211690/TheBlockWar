using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;
public class ReadOnlySimObject(IReadOnlyGameState state, SimulationLayer layer, SimResultList steps, long sourceEventId)
{
    public IReadOnlyGameState State { get; } = state;
    public SimulationLayer Layer { get; } = layer;
    public SimResultList Steps { get; } = steps;
    public long SourceEventId { get; } = sourceEventId;
    public bool IsFocused => Layer == SimulationLayer.Focused;
}

public class SimObject(IGameState state, SimulationLayer layer, SimResultList steps, long sourceEventId)
    : ReadOnlySimObject(state, layer, steps, sourceEventId)
{
    public new IGameState State => (IGameState)base.State;
}

public enum SimulationLayer
{
    Running,//进行实际的模拟
    Focused,//进行复杂的模拟
    Waiting,//不进行复杂的模拟
    Disabled,//已禁用
}