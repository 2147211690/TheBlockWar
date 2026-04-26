using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class EventsView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public GameView GameView { get; set; } = null!;
    [Export] private Node2D _drawContainer = null!;
    [Export] public BlockNumberView BlockNumberView { get; set; } = null!;
    [Export] public BlockBuffView BlockBuffView { get; set; } = null!;
    [Export] public BlockRegionView BlockRegionView { get; set; } = null!;
    public BlockDraw BlockDraw { get; set; } = null!;
    private EventView.EventViewPool _eventViewPool = null!;
    private Dictionary<long, EventView> _eventViews = new();
    private EventView? _currentView;
    private HashSet<long> _clearUnitIds = new();
    
    public override void _Ready()
    {
        _eventViewPool =
            new EventView.EventViewPool(_drawContainer, this);
        BlockDraw = new BlockDraw(this);
    }

    public void ClearEventView(long id)
    {
        if (!_eventViews.TryGetValue(id, out var eventView)) return;
        _eventViewPool.Push(eventView);
        _eventViews.Remove(id);
    }

    public void ClearAll()
    {
        foreach (var eventView in _eventViews.Values) _eventViewPool.Push(eventView);
        _eventViews.Clear();
        BlockDraw.Clear();
        foreach (var id in _clearUnitIds)
        {
            var view = GameView.UnitsView.GetUnitView(id);
            if (view is null) continue;
            view.ClearFacks();
            view.HealthBar.ClearAllChanges();
            view.BuffView.ClearAllBuffChanges();
            view.UnitsView.SetUnitAimCoords(id, null);
        }
        _clearUnitIds.Clear();
    }

    public EventView? BeginAddEventView(long id, bool isFocused, TeamTypeEnum teamType)
    {
        if (_eventViews.ContainsKey(id))
        {
            Logger.Warn($"EventView with id {id} already exists");
            return null;
        }
        if (_currentView != null)
        {
            Logger.Warn($"CurrentView is not null");
            return null;
        }
        _currentView = _eventViewPool.Pop();
        _currentView.IsFocused = isFocused;
        _currentView.TeamType = teamType;
        _eventViews.Add(id, _currentView);
        return _currentView;
    }
    public void NeedClearUnit(long id)
    {
        _clearUnitIds.Add(id);
    }
    
    
    public void EndAddEventView()
    {
        _currentView = null;
    }
    
    
}