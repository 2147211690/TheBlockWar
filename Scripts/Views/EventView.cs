using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;

public class EventView
{
    private EventViewPool _pool;
    private List<DrawNode> _nodes = new();
    private TypedPool<DrawNode> DrawNodePool => _pool.DrawNodePool;
    private TilesView TilesView => _pool.TilesView;
    private EventsStyle Style => _pool.Style;
    private EventView(EventViewPool pool)
    {
        _pool = pool;
    }
    /// <summary>
    /// 放置单位事件视图
    /// </summary>
    /// <param name="coords">放置单位的坐标</param>
    /// <param name="unit">要放置的单位状态</param>
    /// <param name="isValid">当前位置是否可以有效放置单位</param>
    /// <param name="isFocus">是否为关注操作</param>
    public void PlaceUnit(bool isFocus, Vector2I coords, in UnitState unit, bool isValid)
    {
        GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += u =>
        {
            u.DrawTextureOffset(Style.PlaceUnitDown,
                new Vector2(0, -TilesView.GetTileHeight(coords, Layer.Sky)),
                0.5f,
                1f,
                isValid ? Colors.Green : Colors.Red);
        };
        node.Position = TilesView.MapToLocal(coords, Layer.Sky);
        node.QueueRedraw();
    }

    private void Clear()
    {
        foreach (var node in _nodes) DrawNodePool.Push(node);
        _nodes.Clear();
    }
    
    public class EventViewPool : IDisposable
    {
        public TilesView TilesView { get; }
        public TypedPool<DrawNode> DrawNodePool { get; }
        public EventsStyle Style { get; }
        private Node2D _drawContainer;
        private Stack<EventView> _pool = new();
        public EventViewPool(Node2D drawContainer, TilesView tilesView, EventsStyle style)
        {
            _drawContainer = drawContainer;
            TilesView = tilesView;
            Style = style;
            DrawNodePool = new TypedPool<DrawNode>(() => new DrawNode(), 20)
            {
                AfterPush = a =>
                {
                    a.DrawAction = null;
                    a.ProcessRedraw = false;
                    a.Args = null;
                    _drawContainer.RemoveChild(a);
                },
                BeforePop = a =>
                {
                    _drawContainer.AddChild(a);
                    a.Reset();
                }
            };
        }

        public EventView Pop()
        {
            var view = _pool.Count == 0 ? new EventView(this) : _pool.Pop();
            return view;
        }
        public void Push(EventView view)
        { 
            if (view._pool != this) throw new ArgumentException("EventView is not from this pool");
            _pool.Push(view);
            view.Clear();
        }

        public void Dispose()
        {
            DrawNodePool.Dispose();
        }
    }
}