using System.Runtime.CompilerServices;
using System.Threading;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games;

/// <summary>
/// 事件类,用于游戏调度器
/// 事件越小,优先级越高
/// </summary>
public abstract class SchedulerEvent : IComparable<SchedulerEvent>
{
    protected static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public long Id { get; } = YitIdHelper.NextId();
    public SimulationLayer Layer { get; set => SetField(ref field, value); } = SimulationLayer.Waiting;
    public int Team { get; set => SetField(ref field, value); }
    public int Turn { get; set => SetField(ref field, value); } //用于排序
    public int Priority { get; set => SetField(ref field, value); } //用于排序
    public event EventHandler<(string? PropertyName, bool IsChanging)>? PropertyChange;
    public abstract void OnApply(SchedulerEvent root, IGameState state, SimulationContext context);
    public override string ToString() => $"事件Hash:{GetHashCode()} 事件层:{Layer} 事件队伍:{Team} 事件回合:{Turn} 事件优先级:{Priority}";
    protected void OnPropertyChange(string? propertyName, bool isChanging) 
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
}