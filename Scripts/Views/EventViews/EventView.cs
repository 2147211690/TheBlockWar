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
    public bool IsFocused { get; set; }
    public TeamTypeEnum TeamType { get; set; }
    private EventView(EventViewPool pool)
    {
        _pool = pool;
    }
    public void PlaceUnit(PlaceUnitStep step)
    {
        //GD.Print($"PlaceUnit: {coords} {unitName} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                u.DrawTextureOffset(Style[TeamType].PlaceDownTexture,
                    TilesView.GetRenderOffset(step.Coords, Layer.Sky, 0),
                    0.5f,
                    1f,
                    step.ResultIsValid ? Colors.Green : Colors.Red);
            }
        };
        node.Position = TilesView.GetRenderPosition(step.Coords, Layer.Sky, 0);
        node.QueueRedraw();
    }
    public void Damage(DamageStep step)
    {
        ChangeUnitHealth(step.Coords, step.ResultUnitId, -step.Damage, -step.ResultValidDamage);
    }
    public void Cure(CureStep step)
    {
        ChangeUnitHealth(step.Coords, step.ResultUnitId, step.Cure, step.ResultValidCure);
    }
    
    private void ChangeUnitHealth(Vector2I coords, uint unitId, float originChange, float healthChange)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            ChangeUnitHealthDraw(u, v, coords, unitId, originChange, healthChange);
        };
        node.QueueRedraw();
    }

    private void ChangeUnitHealthDraw(DrawNode drawNode, bool visible, Vector2I coords, uint unitId, float originChange ,float healthChange)
    {
        if (visible)
        {
            BlockNumberView.UseBlock(coords);
            if (IsFocused)
            {
                UnitsView.GetUnitView(unitId)?.HealthBar.HealthChange += healthChange;
            }
            BlockNumberView.AddNumber(coords, originChange, unitId == 0 ? originChange : healthChange, unitId != 0, IsFocused);
        }
        else
        {
            BlockNumberView.CancelBlock(coords);
            if (IsFocused)
            {
                UnitsView.GetUnitView(unitId)?.HealthBar.HealthChange -= healthChange;
            }
        }
    }
    
    
    public void MoveUnit(MoveUnitStep step)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var lastAim = UnitsView.GetUnitAimCoords(step.ResultUnitId);
        node.DrawAction += (u, v) =>
        {
            if (step.ResultUnitId == 0) return;
            if (v)
            {
                if (IsFocused)
                {
                    UnitsView.AddUnitFack(step.ResultUnitId, step.Coords);
                    UnitsView.SetUnitAimCoords(step.ResultUnitId, step.To);
                }
                else
                {
                    UnitsView.AddUnitFack(step.ResultUnitId, step.To);
                }
            }
            else
            {
                if (IsFocused)
                {
                    UnitsView.RemoveUnitFack(step.ResultUnitId, step.Coords);
                    UnitsView.SetUnitAimCoords(step.ResultUnitId, lastAim);
                }
                else
                {
                    UnitsView.RemoveUnitFack(step.ResultUnitId, step.To);
                }
            }
        };
        node.QueueRedraw();
    }

    public void MoveUnitPath(MoveUnitPathStep step)
    {
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var lastAim = UnitsView.GetUnitAimCoords(step.ResultUnitId);
        var usedSprites = new Sprite2D[step.Path.Count];
        node.DrawAction += (u, v) =>
        {
            var path = step.Path;
            if (v)
            {
                if (path.Count < 2) return;
                var tex = Style[TeamType].PathTextures[(int)(path[1] - path[0]).ToVecDirMask()];
                SetSprite(step.Coords + path[0], tex, 0);
                for (int i = 1; i < path.Count - 1; i++)
                {
                    tex = Style[TeamType].PathTextures[(int)(path[i - 1] - path[i]).ToVecDirMask() | (int)(path[i + 1] - path[i]).ToVecDirMask()];
                    SetSprite(step.Coords + path[i], tex, i);
                }
                tex = Style[TeamType].PathTextures[(int)DirMask.All ^ (int)(path[^1] - path[^2]).ToVecDirMask()];
                SetSprite(step.Coords + path[^1], tex, path.Count - 1);

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
                    if (usedSprites[i] == null) break;
                    usedSprites[i].Texture = null;
                    usedSprites[i].Offset = Vector2.Zero;
                    SpritePool.Push(usedSprites[i]);
                }
            }
        };
        node.QueueRedraw();
    }
    
    public void DropUnit(DropUnitStep step)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                u.DrawCircle(Vector2.Zero, 10, step.ResultIsValid ? Colors.Green : Colors.Red);
            }
        };
        node.Position = TilesView.GetRenderPosition(step.Coords, Layer.Sky, 0);
        node.QueueRedraw();
    }
    
    public void PushUnit(PushUnitStep step)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                u.DrawTextureCentered(Style[TeamType].PushTextures[step.PushDir.ToIndex()], 
                    TilesView.GetRenderOffset(step.Coords, Layer.Sky, 0),
                    new Color(1,1,1,IsFocused ? 1 : 0.3f));
            }
        };
        node.Position = TilesView.GetRenderPosition(step.Coords, Layer.Sky, 0) +
                        TilesView.GetLocalVec(step.PushDir) * (step.ResultPush ? 0.5f : 0.2f);
        node.QueueRedraw();
    }
    public void ModifyUnitBuff(ModifyUnitBuffStep step)
    {
        //GD.Print($"PlaceUnit: {coords} {unit.Name} {isValid} {isFocus}");
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        var c = step.Operation switch
        {
            BuffOperation.AddBuff => 1,
            BuffOperation.RemoveBuff => -1,
            BuffOperation.ChangeDuration => step.ResultDuration >= 0 ? 1 : -1,
            _ => 0,
        };
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                if (IsFocused)
                {
                    UnitsView.GetUnitView(step.ResultUnitId)?.BuffView.ChangeBuffChange(step.BuffType, c);
                }
            }
            else
            {
                if (IsFocused)
                {
                    UnitsView.GetUnitView(step.ResultUnitId)?.BuffView.ChangeBuffChange(step.BuffType, -c);
                }
            }
        };
        node.Position = TilesView.GetRenderPosition(step.Coords, Layer.Sky, 0);
        node.QueueRedraw();
    }

    public void Animation(AnimationStep step)
    {
        //TODO:绘制动画
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