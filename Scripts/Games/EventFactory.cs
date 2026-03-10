namespace TheBlockWar.Scripts.Games;

public static class EventFactory
{
    private static uint _eventId = 0;
    public static uint CreateEventId()
    {
        return _eventId++;
    }

    public static SchedulerEvnet CreateEvent(
        SchedulerEvnet.EventAction? action = null,
        SimulationLayer layer = SimulationLayer.Running,
        int priority = 0,
        object? args = null)
    {
        return new SchedulerEvnet(CreateEventId())
        {
            Layer = layer,
            Priority = priority,
            Args = args,
            Action = action,
        };
    }
}