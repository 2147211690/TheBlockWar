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
    private  UnitsView UnitsView => _pool.UnitsView;
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
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if(v)
            {
                u.DrawTextureOffset(Style.PlaceUnitDown,
                    new Vector2(0, -TilesView.GetTileHeight(coords, Layer.Sky)),
                    0.5f,
                    1f,
                    isValid ? Colors.Green : Colors.Red);
            }
        };
        node.Position = TilesView.MapToLocal(coords, Layer.Sky);
        node.QueueRedraw();
    }

    public void ChangeUnitHealth(bool isFocus, uint unitId, float healthChange)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            UnitsView.CurrentUnitId = unitId;
            if (v)
            {
                if (isFocus)
                {
                    UnitsView.CurrentUnitHealthChange += healthChange;
                }
            }
            else
            {
                if (isFocus)
                {
                    UnitsView.CurrentUnitHealthChange -= healthChange;
                }
            }
        };
        node.QueueRedraw();
    }
    public void MoveUnit(bool isFocus, uint unitId, Vector2I from, Vector2I to, bool isValid)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        UnitsView.CurrentUnitId = unitId;
        var lastAim = UnitsView.CurrentUnitAimCoords;
        node.DrawAction += (u, v) =>
        {
            if (unitId == 0) return;
            UnitsView.CurrentUnitId = unitId;
            if (v)
            {
                if (isFocus)
                {
                    UnitsView.CurrentAddUnitFack(from);
                    UnitsView.CurrentUnitAimCoords = to;
                }
                else
                {
                    UnitsView.CurrentAddUnitFack(to);
                }
            }
            else
            {
                if (isFocus)
                {
                    UnitsView.CurrentRemoveUnitFack(from);
                    UnitsView.CurrentUnitAimCoords = lastAim;
                }
                else
                {
                    UnitsView.CurrentRemoveUnitFack(to);
                }
            }
        };
        node.QueueRedraw();
    }
    private void Clear()
    {
        for(int i = _nodes.Count - 1; i >= 0; i--) DrawNodePool.Push(_nodes[i]);
        _nodes.Clear();
    }
    
    public class EventViewPool : IDisposable
    {
        public TilesView TilesView { get; }
        public UnitsView UnitsView { get; }
        public TypedPool<DrawNode> DrawNodePool { get; }
        public EventsStyle Style { get; }
        private Node2D _drawContainer;
        private Stack<EventView> _eventViewPool = new();
        public EventViewPool(Node2D drawContainer, TilesView tilesView, EventsStyle style, UnitsView unitsView)
        {
            _drawContainer = drawContainer;
            TilesView = tilesView;
            Style = style;
            UnitsView = unitsView;
            DrawNodePool = new TypedPool<DrawNode>(() => new DrawNode(), 20)
            {
                AfterPush = a =>
                {
                    a.Visible = false;
                    a.DrawAction = null;
                    a.ProcessRedraw = false;
                    a.Args = null;
                    _drawContainer.RemoveChild(a);
                },
                BeforePop = a =>
                {
                    _drawContainer.AddChild(a);
                    a.Reset();
                    a.Visible = true;
                }
            };
        }

        public EventView Pop()
        {
            var view = _eventViewPool.Count == 0 ? new EventView(this) : _eventViewPool.Pop();
            return view;
        }
        public void Push(EventView view)
        { 
            if (view._pool != this) throw new ArgumentException("EventView is not from this pool");
            _eventViewPool.Push(view);
            view.Clear();
        }

        public void Dispose()
        {
            DrawNodePool.Dispose();
        }
    }
}