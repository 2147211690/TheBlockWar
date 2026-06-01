using TheBlockWar.Scripts.Games;
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
    private BlockDraw BlockDraw => _pool.EventsView.BlockDraw;
    public bool IsFocused { get; set; }
    public TeamTypeEnum TeamType { get; set; }
    private EventView(EventViewPool pool)
    {
        _pool = pool;
    }
    private void NeedClearUnit(long id)
    {
        _pool.EventsView.NeedClearUnit(id);
    }
    public void PlaceUnit(PlaceUnitStep step)
    {
        Draw((u, v) =>
            {
                if (v)
                {
                    u.DrawTextureOffset(Style[TeamType].PlaceDownTexture,
                        TilesView.GetRenderOffset(step.Apply.Coords, Layer.Sky, 0),
                        0.5f,
                        1f,
                        step.Valid ? Colors.Green : Colors.Red);
                }
            },
            TilesView.GetRenderPosition(step.Apply.Coords, Layer.Sky, 0));
    }
    public void Damage(DamageStep step)
    {
        ChangeUnitHealth(step.Apply.Coords, step.UnitId, -step.Apply.Damage, -step.ValidDamage, step.UnitId != 0);
    }
    public void Cure(CureStep step)
    {
        ChangeUnitHealth(step.Apply.Coords, step.UnitId, step.ValidCure, step.ValidCure, step.UnitId != 0);
    }

    public void Death(DeathStep step)
    {
        if (IsFocused)
        {
            var bar = UnitsView.GetUnitView(step.UnitId)?.HealthBar;
            if (bar == null) return;
            bar.HealthChange -= bar.CurrentHealth;
            NeedClearUnit(step.UnitId);
        }
        Draw((u, v) =>
        {
            if (v)
            {
                u.DrawTextureCentered(Style[TeamType].DeathTexture, 
                    TilesView.GetRenderOffset(step.Apply.Coords, Layer.Sky, 0));
            }
        },TilesView.GetRenderPosition(step.Apply.Coords, Layer.Sky, 0));
    }
    
    private void ChangeUnitHealth(Vector2I coords, long unitId, float originChange, float healthChange, bool hasAim)
    {
        if (IsFocused)
        {
            UnitsView.GetUnitView(unitId)?.HealthBar.HealthChange += healthChange;
            NeedClearUnit(unitId);
        }
        BlockDraw.AddHealthChange(coords, originChange, healthChange, hasAim, IsFocused, TeamType);
    }
    
    public void MoveUnit(MoveUnitStep step)
    {
        if (step.UnitId == 0) return;
        if (IsFocused)
        {
            UnitsView.AddUnitFack(step.UnitId, step.Apply.Coords);
            UnitsView.SetUnitAimCoords(step.UnitId, step.Apply.To);
        }
        else
        {
            UnitsView.AddUnitFack(step.UnitId, step.Apply.To);
        }
        NeedClearUnit(step.UnitId);
    }

    public void MoveUnitPath(MoveUnitPathStep step)
    {
        var usedSprites = new Sprite2D[step.Apply.Path.Count];
        Draw((u, v) =>
            {
                var path = step.Apply.Path;
                if (v)
                {
                    if (path.Count < 2) return;
                    var tex = Style[TeamType].PathTextures[(int)(path[1] - path[0]).ToVecDirMask()];
                    SetSprite(step.Apply.Coords + path[0], tex, 0);
                    for (int i = 1; i < path.Count - 1; i++)
                    {
                        tex = Style[TeamType]
                            .PathTextures[
                                (int)(path[i - 1] - path[i]).ToVecDirMask() |
                                (int)(path[i + 1] - path[i]).ToVecDirMask()];
                        SetSprite(step.Apply.Coords + path[i], tex, i);
                    }

                    tex = Style[TeamType].PathTextures[(int)DirMask.All ^ (int)(path[^1] - path[^2]).ToVecDirMask()];
                    SetSprite(step.Apply.Coords + path[^1], tex, path.Count - 1);

                    void SetSprite(in Vector2I coords, Texture2D tex, int index)
                    {
                        usedSprites[index] = SpritePool.Get();
                        usedSprites[index].Texture = tex;
                        usedSprites[index].Position = TilesView.GetRenderPosition(coords, Layer.Underground, -1);
                        usedSprites[index].Offset = TilesView.GetRenderOffset(coords, Layer.Underground, -1);
                        usedSprites[index].Modulate = Colors.White with { A = IsFocused ? 1 : 0.6f };
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
                        SpritePool.Return(usedSprites[i]);
                    }
                }
            },
            Vector2.Zero);
    }
    
    public void DropUnit(DropUnitStep step)
    {
        Draw((u, v) =>
            {
                if (v)
                {
                    u.DrawCircle(Vector2.Zero, 10, step.Valid ? Colors.Green : Colors.Red);
                }
            },
            TilesView.GetRenderPosition(step.Apply.Coords, Layer.Sky, 0));
    }
    
    public void PushUnit(PushUnitStep step)
    {
        var pos = TilesView.GetRenderPosition(step.Apply.Coords, Layer.Sky, 0) +
                  TilesView.GetLocalVec(step.Apply.Dir) * (step.Valid ? 0.5f : 0.2f);
        Draw((u, v) =>
            {
                if (v)
                {
                    u.DrawTextureCentered(Style[TeamType].PushTextures[step.Apply.Dir.ToIndex()],
                        TilesView.GetRenderOffset(step.Apply.Coords, Layer.Sky, 0),
                        new Color(1, 1, 1, IsFocused ? 1 : 0.3f));
                }
            },
            pos);
    }
    public void ModifyUnitBuff(ModifyUnitBuffStep step)
    {
        var c = step.Operation switch
        {
            BuffOperation.AddBuff => 1,
            BuffOperation.RemoveBuff => -1,
            _ => 0,
        };
        var isAdd = step.Operation switch
        {
            BuffOperation.AddBuff => true,
            BuffOperation.RemoveBuff => false,
            BuffOperation.ChangeDuration => step.Duration >= 0,
            BuffOperation.SetDuration => true,
            _ => false,
        };
        
        if (step.Deep == 0) BlockDraw.AddBuffChange(step.Apply.Coords, step.Apply.BuffName, isAdd, IsFocused, TeamType);
        if (IsFocused)
        {
            UnitsView.GetUnitView(step.UnitId)?.BuffView.ChangeBuffChange(step.Apply.BuffName, c);
        }
        NeedClearUnit(step.UnitId);
    }

    public void Animation(AnimationStep step)
    {
        var node = DrawNodePool.Get();
        _nodes.Add(node);
        switch (step.Apply.Animation)
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
            var dotSpacing = 4f;
            var dotRadius = 1f;
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

    private DrawNode Draw(Action<DrawNode, bool> drawAction, Vector2 position)
    {
        var node = DrawNodePool.Get();
        _nodes.Add(node);
        node.DrawAction += drawAction;
        node.Position = position;
        node.QueueRedraw();
        return node;
    }
    
    private void Clear()
    {
        for (int i = _nodes.Count - 1; i >= 0; i--) DrawNodePool.Return(_nodes[i]);
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