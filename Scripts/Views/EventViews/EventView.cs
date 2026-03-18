using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.EventViews;

public class EventView
{
    
    private EventViewPool _pool;
    private List<DrawNode> _nodes = new();
    private TypedPool<DrawNode> DrawNodePool => _pool.EventsView.GameView.ViewPools.DrawNodePool;
    private TypedPool<Sprite2D> SpritePool => _pool.EventsView.GameView.ViewPools.Sprite2DPool;
    private BlockNumberView BlockNumberView => _pool.EventsView.BlockNumberView;
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
    public void PlaceUnit(bool isFocus, TeamTypeEnum teamType ,Vector2I coords, IReadOnlyUnitState unit, bool isValid)
    {
        //GD.Print($"PlaceUnit: {coords} {unitName} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            PlaceUnitDraw(u, v, isFocus, teamType, coords, unit, isValid);
        };
        node.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0);
        node.QueueRedraw();
    }


    private void PlaceUnitDraw(DrawNode drawNode, bool visible, bool isFocus, TeamTypeEnum teamType,
        Vector2I coords, IReadOnlyUnitState unit, bool isValid)
    {
        if(visible)
        {
            drawNode.DrawTextureOffset(Style[teamType].PlaceDownTexture,
                TilesView.GetRenderOffset(coords, Layer.Sky, 0),
                0.5f,
                1f,
                isValid ? Colors.Green : Colors.Red);
        }
    }
    public void ChangeUnitHealth(bool isFocus, TeamTypeEnum teamType, Vector2I coords, uint unitId, float healthChange)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            ChangeUnitHealthDraw(u, v, isFocus, teamType, coords, unitId, healthChange);
        };
        node.QueueRedraw();
    }

    private void ChangeUnitHealthDraw(DrawNode drawNode, bool visible, bool isFocus, TeamTypeEnum teamType, Vector2I coords, uint unitId, float healthChange)
    {
        if (visible)
        {
            BlockNumberView.UseBlock(coords);
            if (isFocus)
            {
                UnitsView.GetUnitView(unitId)?.HealthBar.HealthChange += healthChange;
            }
            BlockNumberView.AddNumber(coords, healthChange, unitId != 0, isFocus);
        }
        else
        {
            BlockNumberView.CancelBlock(coords);
            if (isFocus)
            {
                UnitsView.GetUnitView(unitId)?.HealthBar.HealthChange -= healthChange;
            }
        }
    }
    
    
    public void MoveUnit(bool isFocus, uint unitId, Vector2I from, Vector2I to, bool isValid)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var lastAim = UnitsView.GetUnitAimCoords(unitId);
        node.DrawAction += (u, v) =>
        {
            MoveUnitDraw(u, v, isFocus, unitId, from, to, isValid, lastAim);
        };
        node.QueueRedraw();
    }

    private void MoveUnitDraw(DrawNode drawNode, bool visible,
        bool isFocus,uint unitId, Vector2I from, Vector2I to, bool isValid, Vector2I? lastAim)
    {
        if (unitId == 0) return;
        if (visible)
        {
            if (isFocus)
            {
                UnitsView.AddUnitFack(unitId, from);
                UnitsView.SetUnitAimCoords(unitId, to);
            }
            else
            {
                UnitsView.AddUnitFack(unitId, to);
            }
        }
        else
        {
            if (isFocus)
            {
                UnitsView.RemoveUnitFack(unitId, from);
                UnitsView.SetUnitAimCoords(unitId, lastAim);
            }
            else
            {
                UnitsView.RemoveUnitFack(unitId, to);
            }
        }
    }

    public void MoveUnitPath(bool isFocus, TeamTypeEnum teamType, uint unitId, Vector2I coords, List<Vector2I> path, int targetIndex)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var lastAim = UnitsView.GetUnitAimCoords(unitId);
        var sprites = new Sprite2D[path.Count];
        node.DrawAction += (u, v) =>
        {
            MoveUnitPathDraw(u, v, isFocus, teamType, unitId, coords, path, targetIndex, lastAim, sprites);
        };
        node.QueueRedraw();
    }

    private void MoveUnitPathDraw(DrawNode drawNode, bool visible, bool isFocus, TeamTypeEnum teamType, uint unitId,
        Vector2I coords, List<Vector2I> path, int targetIndex, Vector2I? lastAim, Sprite2D[] usedSprites)
    {
        //if (unitId == 0) return;
        /*if (targetIndex > 0)
            MoveUnitDraw(drawNode, visible, isFocus, teamType, unitId, coords + path[0],
                coords + path[targetIndex], true, lastAim);*/
        if (visible)
        {
            if (path.Count < 2) return;
            var tex = Style[teamType].PathTextures[(int)(path[1] - path[0]).ToVecDirMask()];
            SetSprite(coords + path[0], tex, 0);
            for (int i = 1; i < path.Count - 1; i++)
            {
                tex = Style[teamType].PathTextures[(int)(path[i - 1] - path[i]).ToVecDirMask() | (int)(path[i + 1] - path[i]).ToVecDirMask()];
                SetSprite(coords + path[i], tex, i);
            }
            tex = Style[teamType].PathTextures[(int)DirMask.All ^ (int)(path[^1] - path[^2]).ToVecDirMask()];
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
            for (int i = usedSprites.Length - 1; i >= 0; i--)
            {
                usedSprites[i].Texture = null;
                usedSprites[i].Offset = Vector2.Zero;
                SpritePool.Push(usedSprites[i]);
            }
        }
        
    }
    
    public void DropUnit(bool isFocus, Vector2I coords, bool isValid)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                u.DrawCircle(Vector2.Zero, 10, isValid ? Colors.Green : Colors.Red);
            }
        };
        node.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0);
        node.QueueRedraw();
    }
    
    public void PushUnit(bool isFocus, TeamTypeEnum teamType, DirMask pushDir, Vector2I coords, bool isValid)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                u.DrawTextureCentered(Style[teamType].PushTextures[pushDir.ToIndex()], 
                    TilesView.GetRenderOffset(coords, Layer.Sky, 0),
                    new Color(1,1,1,isFocus ? 1 : 0.3f));
            }
        };
        node.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0) +
                        TilesView.GetLocalVec(pushDir) * (isValid ? 0.5f : 0.2f);
        node.QueueRedraw();
    }
    public void ModifyUnitBuff(bool isFocus, Vector2I coords, uint unitId, BuffTypeEnum buffType, BuffOperation operation, int change)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var c = operation switch
        {
            BuffOperation.AddBuff => 1,
            BuffOperation.RemoveBuff => -1,
            BuffOperation.ChangeDuration => change >= 0 ? 1 : -1,
            _ => 0,
        };
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                if (isFocus)
                {
                    UnitsView.GetUnitView(unitId)?.BuffView.ChangeBuffChange(buffType, c);
                }
            }
            else
            {
                if (isFocus)
                {
                    UnitsView.GetUnitView(unitId)?.BuffView.ChangeBuffChange(buffType, -c);
                }
            }
        };
        node.Position = TilesView.GetRenderPosition(coords, Layer.Sky, 0);
        node.QueueRedraw();
    }
    private void Clear()
    {
        for (int i = _nodes.Count - 1; i >= 0; i--) DrawNodePool.Push(_nodes[i]);
        _nodes.Clear();
    }
    
    public class EventViewPool
    {
        public EventsView EventsView { get; }
        private Stack<EventView> _eventViewPool = new();
        public EventViewPool(Node2D drawContainer, EventsView eventsView)
        {
            EventsView = eventsView;
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
    }
}