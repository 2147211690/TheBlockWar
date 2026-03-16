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
    private UnitView? _focusedUnitView;
    
    public Vector2I? GetUnitAimCoords(uint unitId) => _unitViews.GetValueOrDefault(unitId)?.AimCoords;
    public void SetUnitAimCoords(uint unitId, Vector2I? aimCoords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        view.AimCoords = aimCoords;
        SetUnitMapPosition(view.WishCoords, view);
    }
    public UnitView? GetUnitView(uint unitId) => _unitViews.GetValueOrDefault(unitId);
    public void FocusUnit(uint unitId)
    {
        _focusedUnitView?.OutlineColor = new Color("#3aa2003c");
        if (!_unitViews.TryGetValue(unitId, out var render)) return;
        _focusedUnitView = render;
        _focusedUnitView.OutlineColor = new Color("#3aa200");
    }
    public async GDTask PlaceUnit(Vector2I coords, UnitState unit, TeamTypeEnum teamType)
    {
        if (!_unitViews.TryGetValue(unit.Id, out var render))
        {
            render = _unitViews[unit.Id] = _unitScene.Instantiate<UnitView>();
            render.UnitsView = this;
            render.TeamType = teamType;
            render.Id = unit.Id;
            
            _nodeContainer.AddChild(render);
            _unitMaps.Add(coords, unit.Id);
        }
        //设置单位位置和动画
        render!.RealCoords = coords;
        SetUnitMapPosition(render.WishCoords, render);
        render.UnitName = unit.Name;
        await render.PlayPlaceDownAnim();
        //血条
        render.HealthBar.MaxHealth = unit.MaxHealth;
        render.HealthBar.CurrentHealth = unit.Health;
        //护盾条
        render.HealthBar.CurrentShield = unit.Shield;
    }
    public GDTask DropUnit(uint unitId)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return GDTask.CompletedTask;
        _unitMaps.Remove(view.RealCoords);
        _nodeContainer.RemoveChild(view);
        _unitViews.Remove(view.Id);
        view.QueueFree();
        return GDTask.CompletedTask;
    }
    
    public void AddUnitFack(uint unitId, Vector2I coords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        var s = view.AddFack(coords, GameView.TilesView);
        _nodeContainer.AddChild(s);
    }

    public void RemoveUnitFack(uint unitId, Vector2I coords)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        view.RemoveFack(coords);
    }
    
    public void MoveUnit(uint unitId, Vector2I to)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        //await GDTask.Delay(1000);
        _unitMaps.Remove(view.RealCoords);
        view.RealCoords = to;
        SetUnitMapPosition(view.WishCoords, view);
        _unitMaps.Add(to, unitId);
    }

    public async GDTask MovePathUnit(uint unitId, Vector2I coords, List<Vector2I> path, int targetIndex)
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
    }

    public async GDTask PushUnit(uint unitId, DirMask dir, bool push)
    {
        if (!_unitViews.TryGetValue(unitId, out var view)) return;
        var coord = view.RealCoords;
        if (push)
        {
            MoveUnit(unitId, coord + dir.ToVec2I());
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