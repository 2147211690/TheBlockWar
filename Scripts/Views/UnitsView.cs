using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class UnitsView : Node2D
{
    [Export] public GameView GameView { get; set; } = null!;
    [Export] public GameViewNodeContainer DrawContainer { get; private set; } = null!;
    [Export] private PackedScene _unitScene = null!;
    private readonly Dictionary<long, UnitView> _unitViews = new();
    private readonly Dictionary<Vector2I, long> _unitMaps = new();
    private bool _queueUpdateUnitPosition = false;
    private UnitView? _focusedUnitView;
    
    public void ClearAllUnit()
    {
        foreach (var render in _unitViews.Values)
        {
            render.QueueFree();
        }
        _unitViews.Clear();
        _unitMaps.Clear();
    }
    public Vector2I? GetUnitAimCoords(long unitId) => _unitViews.GetValueOrDefault(unitId)?.AimCoords;
    public void SetUnitAimCoords(long unitId, Vector2I? aimCoords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        view.AimCoords = aimCoords;
        SetUnitMapPosition(view.WishCoords, view);
    }
    public UnitView? GetUnitView(long unitId) => _unitViews.GetValueOrDefault(unitId);
    public void FocusUnit(long unitId)
    {
        _focusedUnitView?.OutlineColor = new Color("#3aa2003c");
        if (!_unitViews.TryGetValue(unitId, out var render)) return;
        _focusedUnitView = render;
        _focusedUnitView.OutlineColor = new Color("#3aa200");
    }

    public UnitView? CreateOrGetUnit(Vector2I coords, IReadOnlyUnitState unit, TeamTypeEnum teamType)
    {
        if (unit.IsNull()) return null;
        if (!_unitViews.TryGetValue(unit.Id, out var render))
        {
            render = _unitViews[unit.Id] = _unitScene.Instantiate<UnitView>();
            render.Init(GameView, GameView.Style, teamType, this);
            DrawContainer.AddChild(render);
            _unitMaps.Add(coords, unit.Id);
        }
        //设置单位位置和动画
        render.RealCoords = coords;
        SetUnitMapPosition(render.WishCoords, render);
        render.UnitName = unit.Name;
        //血条
        render.HealthBar.MaxHealth = unit.MaxHealth;
        render.HealthBar.CurrentHealth = unit.Health;
        //护盾条
        render.HealthBar.CurrentShield = unit.Shield;
        //Buff
        foreach (var buff in unit.Buffs)
        {
            render.BuffView.AddVisibleBuff(buff.BuffName);
        }
        return render;
    }
    public GDTask PlaceUnit(Vector2I coords, IReadOnlyUnitState unit, TeamTypeEnum teamType)
    {
        var render = CreateOrGetUnit(coords, unit, teamType);
        if (render == null) return GDTask.CompletedTask;
        return render.PlayPlaceDownAnim();
    }
    public async GDTask DropUnit(long unitId)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        var dropHelght = 50f;
        var targetPosition = GameView.TilesView.GetGamePositionOnGround(view.WishCoords, dropHelght);
        //动画
        // 创建动画
        var tween = view.CreateTween();
        // 使用Quad加速过渡，模拟重力加速效果
        tween.TweenProperty(view, "GamePosition", targetPosition, 0.12f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.In);
        
        await tween.WaitForTween();
        //动画完成后
        _unitMaps.Remove(view.RealCoords);
        DrawContainer.RemoveChild(view);
        _unitViews.Remove(view.Id);
        view.QueueFree();
    }

    public void SwapUnit(long u1, long u2)
    {
        if (!_unitViews.TryGetValue(u1, out var view1) || !_unitViews.TryGetValue(u2, out var view2)) return;
        _unitMaps.Remove(view1.RealCoords);
        _unitMaps.Remove(view2.RealCoords);
        (view1.RealCoords, view2.RealCoords) = (view2.RealCoords, view1.RealCoords);
        SetUnitMapPosition(view1.WishCoords, view1);
        SetUnitMapPosition(view2.WishCoords, view2);
        _unitMaps.Add(view1.RealCoords, u1);
        _unitMaps.Add(view2.RealCoords, u2);
    }
    
    public void AddUnitFack(long unitId, Vector2I coords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        view.AddFack(coords);
    }

    public void RemoveUnitFack(long unitId, Vector2I coords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        view.RemoveFack(coords);
    }
    
    public void MoveUnit(long unitId, Vector2I to)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        //await GDTask.Delay(1000);
        _unitMaps.Remove(view.RealCoords);
        view.RealCoords = to;
        SetUnitMapPosition(view.WishCoords, view);
        _unitMaps.Add(to, unitId);
    }

    public async GDTask MovePathUnit(long unitId, Vector2I coords, List<Vector2I> path, int targetIndex)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        _unitMaps.Remove(view.RealCoords);
        SetUnitMapPosition(coords + path[0], view);
        for (int i = 1; i <= targetIndex; i++)
        {
            await GDTask.Delay(100);
            SetUnitMapPosition(coords + path[i], view);
        }
        view.RealCoords = coords + path[targetIndex];
        _unitMaps.Add(coords + path[targetIndex], unitId);
        SetUnitMapPosition(view.WishCoords, view);
        await GDTask.Delay(100);
    }

    public async GDTask PushUnit(long unitId, DirMask dir, bool push)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        var coords = view.RealCoords;
        var tween = view.CreateTween();
        var positionTarget = GameView.TilesView.GetGamePositionOnGround(coords + dir.ToVec2I());
        if (push)
        {
            tween.TweenProperty(view, "GamePosition", positionTarget, 0.08f);
            await tween.WaitForTween();
            MoveUnit(unitId, coords + dir.ToVec2I());
        }
        else
        {
            // 撞击弹回动画
            var pushRatio = 0.5f; // 前冲比例
            var pushTime = 0.05f; // 前冲时间
            var bounceTime = 0.10f; // 弹回时间
    
            // 第一阶段：快速前冲到30%位置
            var initialPos = view.GamePosition;
    
            // 计算中间位置（当前到目标的30%）
            var intermediatePos = initialPos + (positionTarget - initialPos) * pushRatio;
    
            tween.TweenProperty(view, "GamePosition", intermediatePos, pushTime)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            
            // 第二阶段：有力弹回原位
            tween.Chain()
                .TweenProperty(view, "GamePosition", initialPos, bounceTime)
                .SetTrans(Tween.TransitionType.Bounce)
                .SetEase(Tween.EaseType.Out);
    
            await tween.WaitForTween();
            // 不调用MoveUnit，单位停留在原位置
        }
    }
    
    /// <summary>
    /// 对于会影响瓦片高度的操作，需要调用此方法更新单位位置
    /// </summary>
    public void QueueUpdateUnitPosition()
    {
        _queueUpdateUnitPosition = true;
    }

    public void UpdateUnitPosition(Vector2I coords)
    {
        if (!_unitMaps.TryGetValue(coords, out var unitId)) return;
        SetUnitMapPosition(coords, _unitViews[unitId]);
    }
    private void SetUnitMapPosition(Vector2I coords, UnitView render)
    {
        render.GamePosition = GameView.TilesView.GetGamePositionOnGround(coords);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        if (_queueUpdateUnitPosition)
        {
            _queueUpdateUnitPosition = false;
            foreach (var unit in _unitMaps)
            {
                SetUnitMapPosition(unit.Key, _unitViews[unit.Value]);
            }
        }
    }

}