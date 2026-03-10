using Godot;
using Godot.Collections;
using GodotTask;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class UnitsView : Node2D
{
    [Export] private Node2D _nodeContainer;
    [Export] private PackedScene _unitScene;
    [Export] public TilesView TilesView { get; set; }
    private readonly Dictionary<uint, UnitView> _unitViews = new();
    private readonly Dictionary<Vector2I, uint> _unitMaps = new();
    private bool _queueUpdateUnitPosition = false;
    public GDTask PlaceUnit(Vector2I coords, UnitState unit)
    {
        if (!_unitViews.TryGetValue(unit.Id, out var render))
        {
            render = _unitViews[unit.Id] = _unitScene.Instantiate<UnitView>();
            _nodeContainer.AddChild(render);
            _unitMaps.Add(coords, unit.Id);
        }
        SetUnitMapPosition(coords, render);
        render.UnitName = unit.Name;
        render.PlayPlaceDownAnim();
        return GDTask.CompletedTask;
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
        render.Position = TilesView.MapToLocal(coords, Layer.Underground);
        render.SetGroundHeight(TilesView.GetTileHeight(coords, Layer.Underground));
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