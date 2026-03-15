using System.Collections.Generic;
using Godot;
using GodotTask;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class UnitsView : Node2D
{
    [Export] public GameView GameView { get; set; }
    [Export] private Node2D _nodeContainer;
    [Export] private PackedScene _unitScene;
    private readonly Godot.Collections.Dictionary<uint, UnitView> _unitViews = new();
    private readonly Godot.Collections.Dictionary<Vector2I, uint> _unitMaps = new();
    private bool _queueUpdateUnitPosition = false;
    private UnitView? _currentUnitView;
    private UnitView? _focusedUnitView;
    public uint CurrentUnitId
    {
        get;
        set
        {
            field = value;
            _unitViews.TryGetValue(value, out _currentUnitView);
        }
    }

    public Vector2I? CurrentUnitAimCoords
    {
        get => _currentUnitView?.AimCoords;
        set
        {
            if (_currentUnitView is null) return;
            _currentUnitView.AimCoords = value; 
            SetUnitMapPosition(_currentUnitView.WishCoords, _currentUnitView);
        }
    }

    public float CurrentUnitHealthChange
    { get => _currentUnitView?.HealthBar.HealthChange ?? 0; set => _currentUnitView?.HealthBar.HealthChange = value; }
    public float CurrentUnitShieldChange
    { get => _currentUnitView?.HealthBar.ShieldChange ?? 0; set => _currentUnitView?.HealthBar.ShieldChange = value; }
    public float CurrentUnitShield
    { get => _currentUnitView?.HealthBar.CurrentShield ?? 0; set => _currentUnitView?.HealthBar.CurrentShield = value; }
    public float CurrentUnitHealth
    { get => _currentUnitView?.HealthBar.CurrentHealth ?? 0; set => _currentUnitView?.HealthBar.CurrentHealth = value; }
    
    

    public void FocusUnit(uint unitId)
    {
        _focusedUnitView?.OutlineColor = new Color("#3aa2003c");
        if (!_unitViews.TryGetValue(unitId, out var render)) return;
        _focusedUnitView = render;
        _focusedUnitView.OutlineColor = new Color("#3aa200");
    }
    public GDTask PlaceUnit(Vector2I coords, UnitState unit, TeamTypeEnum teamType)
    {
        if (!_unitViews.TryGetValue(unit.Id, out var render))
        {
            render = _unitViews[unit.Id] = _unitScene.Instantiate<UnitView>();
            render.UnitsView = this;
            render.TeamType = teamType;
            render.Id = unit.Id;
            
            _nodeContainer.AddChild(render);
            _unitMaps.Add(coords, unit.Id);
            if (unit.Id == CurrentUnitId) _currentUnitView = render;
        }
        //设置单位位置和动画
        render!.RealCoords = coords;
        SetUnitMapPosition(render.WishCoords, render);
        render.UnitName = unit.Name;
        render.PlayPlaceDownAnim();
        //血条
        render.HealthBar.MaxHealth = unit.MaxHealth;
        render.HealthBar.CurrentHealth = unit.Health;
        //护盾条
        render.HealthBar.CurrentShield = unit.Shield;
        
        return GDTask.CompletedTask;
    }
    public GDTask CurrentDropUnit()
    {
        if (_currentUnitView == null) return GDTask.CompletedTask;
        _unitMaps.Remove(_currentUnitView.RealCoords);
        _nodeContainer.RemoveChild(_currentUnitView);
        _unitViews.Remove(_currentUnitView.Id);
        _currentUnitView.QueueFree();
        _currentUnitView = null;
        return GDTask.CompletedTask;
    }
    
    public void CurrentAddUnitFack(Vector2I coords)
    {
        if (_currentUnitView is null) return;
        var s = _currentUnitView!.AddFack(coords, GameView.TilesView);
        _nodeContainer.AddChild(s);
    }

    public void CurrentRemoveUnitFack(Vector2I coords)
    {
        _currentUnitView?.RemoveFack(coords);
    }
    
    public void CurrentMoveUnit(Vector2I to)
    {
        if (_currentUnitView is null) return;
        //await GDTask.Delay(1000);
        _unitMaps.Remove(_currentUnitView!.RealCoords);
        _currentUnitView.RealCoords = to;
        SetUnitMapPosition(_currentUnitView.WishCoords, _currentUnitView);
        _unitMaps.Add(to, CurrentUnitId);
    }

    public async GDTask CurrentMovePathUnit(Vector2I coords, List<Vector2I> path, int targetIndex)
    {
        if (_currentUnitView is null) return;
        _unitMaps.Remove(_currentUnitView!.RealCoords);
        SetUnitMapPosition(coords + path[0], _currentUnitView);
        for (int i = 1; i <= targetIndex; i++)
        {
            await GDTask.Delay(100);
            SetUnitMapPosition(coords + path[i], _currentUnitView);
        }
        _currentUnitView.RealCoords = coords + path[targetIndex];
        _unitMaps.Add(coords + path[targetIndex], CurrentUnitId);
        SetUnitMapPosition(_currentUnitView.WishCoords, _currentUnitView);
    }

    public async GDTask CurrentPushUnit(DirMask dir, bool push)
    {
        if (_currentUnitView is null) return;
        var coord = _currentUnitView!.RealCoords;
        if (push)
        {
            CurrentMoveUnit(coord + dir.ToVec2I());
        }
        else
        {
            
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
        render.Position = GameView.TilesView.GetRenderPosition(coords, Layer.Underground, 0);
        render.Offset = GameView.TilesView.GetRenderOffset(coords, Layer.Underground, 0);
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