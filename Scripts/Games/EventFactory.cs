using System;
using Godot;

namespace TheBlockWar.Scripts.Games;

public static class EventFactory
{
    private static uint _eventId = 1;
    public static uint CreateEventId()
    {
        return _eventId++;
    }

    public static SchedulerEvent CreateEvent(
        ISchedulerEvnet.EventAction? action = null,
        int priority = 0,
        object? args = null)
    {
        return new SchedulerEvent(CreateEventId())
        {
            Priority = priority,
            Args = args,
            Action = action,
        };
    }
    public static SkillEvent CreateSkill(
        uint bindUnitId,
        ISkill? skill = null,
        int priority = int.MaxValue)
    {
        return new SkillEvent(CreateEventId(), skill)
        {
            BindUnitId = bindUnitId,
            Priority = priority,
        };
    }
}