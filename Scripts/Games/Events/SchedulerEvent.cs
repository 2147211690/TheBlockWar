using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Events;

/// <summary>
/// 事件类,用于游戏调度器
/// 事件越小,优先级越高
/// </summary>
[JsonDerivedType(typeof(ActionEvent), nameof(ActionEvent))]
[JsonDerivedType(typeof(SkillEvent), nameof(SkillEvent))]
[JsonDerivedType(typeof(TurnEvent), nameof(TurnEvent))]
[JsonDerivedType(typeof(StepsEvent), nameof(StepsEvent))]
public abstract class SchedulerEvent(long id) : IComparable<SchedulerEvent>
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public long Id { get; } = id;
    public SimulationLayer Layer { get; set => SetField(ref field, value); } = SimulationLayer.Waiting;
    public int Team { get; set => SetField(ref field, value); }
    public int Turn { get; set => SetField(ref field, value); } //用于排序
    public int Priority { get; set => SetField(ref field, value); } //用于排序
    
    public event EventHandler<(string? PropertyName, bool IsChanging)>? PropertyChange;

    public override string ToString() => 
        $"Id:{Id} Layer:{Layer} Team:{Team} Turn:{Turn} Priority:{Priority}";
    private void OnPropertyChange(string? propertyName, bool isChanging) 
        => PropertyChange?.Invoke(this, (propertyName, isChanging));
    protected bool SetField<T>(ref T field, in T value, bool checkEqual = true, [CallerMemberName]string? propertyName = null)
    {
        if (checkEqual && EqualityComparer<T>.Default.Equals(field, value)) return false;
        OnPropertyChange(propertyName, true);
        field = value;
        OnPropertyChange(propertyName, false);
        return true;
    }

    public int CompareTo(SchedulerEvent? other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is null) return 1;
        var layerComparison = Layer.CompareTo(other.Layer);
        if (layerComparison != 0) return layerComparison;
        var turnComparison = Turn.CompareTo(other.Turn);
        if (turnComparison != 0) return turnComparison;
        var priorityComparison = Priority.CompareTo(other.Priority);
        if (priorityComparison != 0) return priorityComparison;
        return Id.CompareTo(other.Id);
    }
    public abstract void OnApply(SchedulerEvent root, IGameState state, SimulationContext context);
    public static int DefaultCompare(SchedulerEvent a, SchedulerEvent b) => a.CompareTo(b);
    public static int IgnoreLayerCompare(SchedulerEvent a, SchedulerEvent b)
    {
        if (ReferenceEquals(a, b)) return 0;
        var turnComparison = a.Turn.CompareTo(b.Turn);
        if (turnComparison != 0) return turnComparison;
        var priorityComparison = a.Priority.CompareTo(b.Priority);
        if (priorityComparison != 0) return priorityComparison;
        return a.Id.CompareTo(b.Id);
    }
}