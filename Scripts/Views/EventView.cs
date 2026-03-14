using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;

public class EventView
{
    
    private EventViewPool _pool;
    private List<DrawNode> _nodes = new();
    private TypedPool<DrawNode> DrawNodePool => _pool.DrawNodePool;
    private TypedPool<Sprite2D> SpritePool => _pool.SpritePool;
    private TilesView TilesView => _pool.EventsView.GameView.TilesView;
    private GameViewStyle Style => _pool.EventsView.GameView.Style;
    private UnitsView UnitsView => _pool.EventsView.GameView.UnitsView;
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
    public void PlaceUnit(bool isFocus, Vector2I coords, UnitState unit, bool isValid)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            PlaceUnitDraw(u, v, isFocus, coords, unit, isValid);
        };
        node.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0);
        node.QueueRedraw();
    }

    private void PlaceUnitDraw(DrawNode drawNode, bool visible, bool isFocus, Vector2I coords, UnitState unit, bool isValid)
    {
        if(visible)
        {
            drawNode.DrawTextureOffset(Style.PlaceDownTexture,
                TilesView.GetRenderOffset(coords, Layer.Sky, 0),
                0.5f,
                1f,
                isValid ? Colors.Green : Colors.Red);
        }
    }
    public void ChangeUnitHealth(bool isFocus, uint unitId, float healthChange)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            ChangeUnitHealthDraw(u, v, isFocus, unitId, healthChange);
        };
        node.QueueRedraw();
    }

    private void ChangeUnitHealthDraw(DrawNode drawNode, bool visible, bool isFocus, uint unitId, float healthChange)
    {
        var last = UnitsView.CurrentUnitId;
        UnitsView.CurrentUnitId = unitId;
        if (visible)
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
        UnitsView.CurrentUnitId = last;
    }
    
    
    public void MoveUnit(bool isFocus, uint unitId, Vector2I from, Vector2I to, bool isValid)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        UnitsView.CurrentUnitId = unitId;
        var lastAim = UnitsView.CurrentUnitAimCoords;
        node.DrawAction += (u, v) =>
        {
            MoveUnitDraw(u, v, isFocus, unitId, from, to, isValid, lastAim);
        };
        node.QueueRedraw();
    }

    private void MoveUnitDraw(DrawNode drawNode, bool visible,
        bool isFocus, uint unitId, Vector2I from, Vector2I to, bool isValid, Vector2I? lastAim)
    {
        if (unitId == 0) return;
        var last = UnitsView.CurrentUnitId;
        UnitsView.CurrentUnitId = unitId;
        if (visible)
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
        UnitsView.CurrentUnitId = last;
    }

    public void MoveUnitPath(bool isFocus, uint unitId, Vector2I coords, List<Vector2I> path, int targetIndex)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var lastAim = UnitsView.CurrentUnitAimCoords;
        var sprites = new Sprite2D[path.Count];
        node.DrawAction += (u, v) =>
        {
            MoveUnitPathDraw(u, v, isFocus, unitId, coords, path, targetIndex, lastAim, sprites);
        };
        node.QueueRedraw();
    }

    private void MoveUnitPathDraw(DrawNode drawNode, bool visible, bool isFocus, uint unitId,
        Vector2I coords, List<Vector2I> path, int targetIndex, Vector2I? lastAim, Sprite2D[] usedSprites)
    {
        if (unitId == 0) return;
        var last = UnitsView.CurrentUnitId;
        UnitsView.CurrentUnitId = unitId;
        if (targetIndex > 0)
            MoveUnitDraw(drawNode, visible, isFocus, unitId, coords + path[0],
                coords + path[targetIndex], true, lastAim);
        if (visible)
        {
            if (path.Count < 2) return;
            var tex = Style.PathTextures[(int)(path[1] - path[0]).ToVecDirMask()];
            SetSprite(coords + path[0], tex, 0);
            for (int i = 1; i < path.Count - 1; i++)
            {
                tex = Style.PathTextures[(int)(path[i - 1] - path[i]).ToVecDirMask() | (int)(path[i + 1] - path[i]).ToVecDirMask()];
                SetSprite(coords + path[i], tex, i);
            }
            tex = Style.PathTextures[(int)DirMask.All ^ (int)(path[^1] - path[^2]).ToVecDirMask()];
            SetSprite(coords + path[^1], tex, path.Count - 1);

            void SetSprite(in Vector2I coords, Texture2D tex, int index)
            {
                usedSprites[index] = SpritePool.Pop();
                usedSprites[index].Texture = tex;
                usedSprites[index].Position = TilesView.GetRenderPosition(coords , Layer.Underground, -1);
                usedSprites[index].Offset = TilesView.GetRenderOffset(coords, Layer.Underground, -1);
            }
        }
        else
        {
            for (int i = usedSprites.Length - 1; i >= 0; i--) SpritePool.Push(usedSprites[i]);
        }
        UnitsView.CurrentUnitId = last;
        
    }
    
    
    
    private void Clear()
    {
        for (int i = _nodes.Count - 1; i >= 0; i--) DrawNodePool.Push(_nodes[i]);
        _nodes.Clear();
    }
    
    public class EventViewPool : IDisposable
    {
        public EventsView EventsView { get; }
        public TypedPool<DrawNode> DrawNodePool { get; }
        public TypedPool<Sprite2D> SpritePool { get; }
        private Node2D _drawContainer;
        private Stack<EventView> _eventViewPool = new();
        public EventViewPool(Node2D drawContainer, EventsView eventsView)
        {
            _drawContainer = drawContainer;
            EventsView = eventsView;
            DrawNodePool = new TypedPool<DrawNode>(() =>
            {
                var node = new DrawNode();
                _drawContainer.AddChild(node);
                return node;
            }, 20)
            {
                AfterPush = a =>
                {
                    a.Visible = false;
                    a.DrawAction = null;
                    a.ProcessRedraw = false;
                    a.Args = null;
                    //_drawContainer.RemoveChild(a);
                },
                BeforePop = a =>
                {
                    //_drawContainer.AddChild(a);
                    a.Reset();
                    a.Visible = true;
                }
            };
            SpritePool = new TypedPool<Sprite2D>(() =>
            {
                var node = new Sprite2D();
                _drawContainer.AddChild(node);
                return node;
            }, 5)
            {
                AfterPush = a =>
                {
                    a.Visible = false;
                    a.Texture = null;
                    a.Offset = Vector2.Zero;
                    //_drawContainer.RemoveChild(a);
                },
                BeforePop = a =>
                {
                    //_drawContainer.AddChild(a);
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
            SpritePool.Dispose();
        }
    }
}