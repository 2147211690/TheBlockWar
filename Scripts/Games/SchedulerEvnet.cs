using System;
using System.Collections.Generic;

namespace TheBlockWar.Scripts.Games;

public class SchedulerEvnet : IComparable<SchedulerEvnet>
{
    public SchedulerEvnet(uint id)
    {
        Id = id;
    }
    public uint Id { get; }
    public SimulationLayer Layer { get => field; set => SetField(ref field, value); }
    public int Priority { get => field; set => SetField(ref field, value); }
    public object? Args { get => field; set => SetField(ref field, value); }
    public EventAction? Action { get => field; set => SetField(ref field, value); }
    public void UpdateEvent() => Update?.Invoke(this, EventArgs.Empty);
    private void SetField<T>(ref T field, in T value)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        Update?.Invoke(this, EventArgs.Empty);
    }
    public event EventHandler? Update;
    
    public delegate void EventAction(IGameState state, object? args, SimulationResult result);
    
    public static bool operator <(SchedulerEvnet a, SchedulerEvnet b) => a.CompareTo(b) < 0;
    public static bool operator >(SchedulerEvnet a, SchedulerEvnet b) => a.CompareTo(b) > 0;

    public int CompareTo(SchedulerEvnet? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        if (Layer != other.Layer) return other.Layer.CompareTo(Layer);
        return -Priority.CompareTo(other.Priority);
    }
}