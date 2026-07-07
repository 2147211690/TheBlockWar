using System.Runtime.CompilerServices;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.GameStates;

public class EventState
{


    public static readonly Comparer<EventState> OrderComparer = Comparer<EventState>.Create(OrderCompare);
    public static readonly Comparer<EventState> SimulationComparer = Comparer<EventState>.Create(SimulationCompare);
    
    public long Id { get; private set; }
    public Vector2I? Coords => Core?.GetCoords(this);
    public long BindUnitId => Core?.GetBindUnitId(this) ?? 0;
    public int Team { get; set => SetProperty(ref field, value); }
    public SimulationLayer Layer { get; set => SetProperty(ref field, value); }
    public int PhaseOrder { get; set => SetProperty(ref field, value); }
    public int Priority { get; set => SetProperty(ref field, value); }
    public IEventCore? Core { get; set => SetProperty(ref field, value); }
    public EventAfterRunMode AfterRunMode { get; set; }
    
    public event EventHandler<(string PropertyName, bool IsChanging)>? PropertyChanged;

    public EventState(long id)
    {
        Id = id;
    }
    
    private void SetProperty<T>(ref T field, in T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        PropertyChanged?.Invoke(this, (propertyName, true));
        field = value;
        PropertyChanged?.Invoke(this, (propertyName, false));
    }
    
    public void Apply(SimObject obj)
    {
        Core?.OnApply(this, obj);
    }
    public bool CheckIsValid(IReadOnlyGameState gameState)
    {
        return Core?.CheckIsValid(this, gameState) ?? false;
    }
    
    public static int OrderCompare(EventState a, EventState b)
    {
        var phaseOrderCompare = a.PhaseOrder.CompareTo(b.PhaseOrder);
        if (phaseOrderCompare != 0) return phaseOrderCompare;
        return a.Priority.CompareTo(b.Priority);
    }
    public static int SimulationCompare(EventState a, EventState b)
    {
        var layerCompare = a.Layer.CompareTo(b.Layer);
        if (layerCompare != 0) return layerCompare;
        return OrderCompare(a, b);
    }

    public void Clear()
    {
        Core = null;
        PropertyChanged = null;
    }
    public void CloneTo(EventState target)
    {
        target.Id = Id;
        target.Team = Team;
        target.Layer = Layer;
        target.PhaseOrder = PhaseOrder;
        target.Priority = Priority;
        target.Core = Core;
        target.AfterRunMode = AfterRunMode;
    }
    public EventState Clone()
    {
        var result = new EventState(Id);
        CloneTo(result);
        return result;
    }
}
public enum EventAfterRunMode
{
    Remove,
    ToFocused,
    ToWaiting,
    ToDisabled,
}