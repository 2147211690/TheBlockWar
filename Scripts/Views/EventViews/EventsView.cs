using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Views.EventViews;
[GlobalClass]
public partial class EventsView : Node2D
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public GameView GameView { get; set; }
    [Export] private Node2D _drawContainer;
    [Export] public BlockNumberView BlockNumberView { get; set; }
    [Export] public BlockBuffView BlockBuffView { get; set; }
    [Export] public BlockRegionView BlockRegionView { get; set; }
    public BlockDraw BlockDraw { get; set; }
    private EventView.EventViewPool _eventViewPool;
    private Dictionary<uint, EventView> _eventViews = new();
    private EventView? _currentView;
    private HashSet<uint> _clearUnitIds = new();
    
    public override void _Ready()
    {
        _eventViewPool =
            new EventView.EventViewPool(_drawContainer, this);
        BlockDraw = new BlockDraw(this);
    }

    public void ClearEventView(uint id)
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

    public EventView? BeginAddEventView(uint id, bool isFocused, TeamTypeEnum teamType)
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
    public void NeedClearUnit(uint id)
    {
        _clearUnitIds.Add(id);
    }
    
    
    public void EndAddEventView()
    {
        _currentView = null;
    }
    
    
}