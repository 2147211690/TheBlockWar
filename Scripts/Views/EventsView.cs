using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class EventsView : Node2D
{
    [Export] public TilesView TilesView { get; set; }
    [Export] private Node2D _drawContainer;
    [Export] private EventsStyle _style;
    private EventView.EventViewPool _eventViewPool;
    private Dictionary<uint, EventView> _eventViews = new();
    private EventView? _currentView;

    public override void _Ready()
    {
        _eventViewPool = new EventView.EventViewPool(_drawContainer, TilesView, _style);
        TreeExiting += () =>
        {
            _eventViewPool.Dispose();
        };
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
    }

    public EventView? BeginAddEventView(uint id)
    {
        if (_eventViews.ContainsKey(id) || _currentView != null)
        {
            GD.PrintErr($"EventView with id {id} already exists or CurrentView is not null");
            return null;
        }
        _currentView = _eventViewPool.Pop();
        _eventViews.Add(id, _currentView);
        return _currentView;
    }

    public void EndAddEventView()
    {
        _currentView = null;
    }
    
    
}