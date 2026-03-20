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
    private BlockBuffView BlockBuffView => _pool.EventsView.BlockBuffView;
    private BlockRegionView BlockRegionView => _pool.EventsView.BlockRegionView;
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
            BlockNumberView.RemoveNumber(coords, originChange, unitId == 0 ? originChange : healthChange, unitId != 0, IsFocused);
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
                    usedSprites[index].Modulate = Colors.White with {A = IsFocused ? 1 : 0.6f};
                }
            }
            else
            {
                for (int i = usedSprites.Length - 1; i >= 0; i--)
                {
                    if (usedSprites[i] == null) break;
                    usedSprites[i].Texture = null;
                    usedSprites[i].Offset = Vector2.Zero;
                    usedSprites[i].Modulate = Colors.White;
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
        var color = step.Operation switch
        {
            BuffOperation.AddBuff => Colors.White,
            BuffOperation.RemoveBuff => Colors.OrangeRed,
            _ => Colors.White,
        };
        var isAdd = step.Operation switch
        {
            BuffOperation.AddBuff => true,
            BuffOperation.RemoveBuff => false,
            BuffOperation.ChangeDuration => step.Duration >= 0,
            BuffOperation.SetDuration => true,
        };
        color.A = IsFocused ? 1 : 0.6f;
        
        node.DrawAction += (u, v) =>
        {
            if (v)
            {
                BlockBuffView.AddBuffChange(step.Coords,
                    step.BuffType,
                    isAdd,
                    IsFocused
                );
                if (IsFocused)
                {
                    UnitsView.GetUnitView(step.ResultUnitId)?.BuffView.ChangeBuffChange(step.BuffType, c);
                }
            }
            else
            {
                BlockBuffView.RemoveBuffChange(step.Coords,
                    step.BuffType,
                    isAdd,
                    IsFocused
                );
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
        var node = DrawNodePool.Pop();
        _nodes.Add(node);
        switch (step.Animation)
        {
            case ShotSomethingAnim shotSomethingAnim:
                ShotSomething(node, shotSomethingAnim);
                break;
            case ThrowSomethingAnim throwSomethingAnim:
                ThrowSomething(node, throwSomethingAnim);
                break;
            default:
                break;
        }
        node.QueueRedraw();

        void ShotSomething(DrawNode node, ShotSomethingAnim anim)
        {
            node.Position = Vector2.Zero;
            var startPos = TilesView.GetRealPosition(anim.Coords, Layer.Sky);
            var endPos = TilesView.GetRealPosition(anim.To, Layer.Sky);
            var distance = startPos.DistanceTo(endPos);
            var dotSpacing = 20f;
            var dotRadius = 4f;
            var dotCount = (int)(distance / dotSpacing);
            
            node.DrawAction +=
                (u, v) => PointLineDraw(u, v, dotCount, dotRadius, (t) => startPos.Lerp(endPos, t));
        }
        void ThrowSomething(DrawNode node, ThrowSomethingAnim anim)
        {
            node.Position = Vector2.Zero;
            var throwStart = TilesView.GetRealPosition(anim.Coords, Layer.Underground);
            var throwEnd = TilesView.GetRealPosition(anim.To, Layer.Underground);
            var throwCenter = throwEnd.Lerp(throwStart, 0.5f) + new Vector2(0, -anim.ThrowHeight);
    
            var distance = throwStart.DistanceTo(throwEnd);
            var dotSpacing = 20f;
            var dotRadius = 4f;
            var dotCount = (int)(distance / dotSpacing);
    
            node.DrawAction +=
                (u, v) => PointLineDraw(u, v, dotCount, dotRadius, t =>
                {
                    var s1 = throwStart.Lerp(throwCenter, t);
                    var s2 = throwCenter.Lerp(throwEnd, t);
                    return s1.Lerp(s2, t);
                });
        }
    }

    private void PointLineDraw(DrawNode u, bool v, int dotCount, float dotRadius, Func<float, Vector2> getPos)
    {
        if (v)
        {
            u.ZIndex = 1;
            var color = Colors.White with { A = IsFocused ? 1 : 0.6f };

            for (int i = 0; i <= dotCount; i++)
            {
                var t = (float)i / dotCount;
                var dotPos = getPos(t);
                u.DrawCircle(dotPos, dotRadius, color);
            }
        }
        else
        {
            u.ZIndex = 0;
        }
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