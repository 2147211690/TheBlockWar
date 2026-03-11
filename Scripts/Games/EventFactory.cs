using System;
using Godot;

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
    public static SchedulerEvnet CreateSkill(
        uint bindUnitId,
        ISkill skill = null,
        SimulationLayer layer = SimulationLayer.Waiting,
        int priority = 0)
    {
        return new SchedulerEvnet(CreateEventId())
        {
            Layer = layer,
            Priority = priority,
            Args = null,
            Action = (s, a, r) =>
            {
                var unit = s.GetUnit(bindUnitId);
                if (unit == null) return;
                skill.SkillCore(s, r, unit.Value, a as Vector2I[] ?? []);
            },
        };
    }
}