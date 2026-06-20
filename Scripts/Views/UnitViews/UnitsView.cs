using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.UnitViews;
[GlobalClass]
public partial class UnitsView : Node2D
{
    public IGameView GameView { get; set; } = null!;
    private readonly Dictionary<long, UnitView> _unitViews = new();
    private readonly Dictionary<Vector2I, long> _unitMaps = new();

    public void ClearAllUnit()
    {
        foreach (var render in _unitViews.Values)
        {
            render.QueueFree();
        }
        _unitViews.Clear();
        _unitMaps.Clear();
    }
    public UnitView? GetUnitView(long unitId) => _unitViews.GetValueOrDefault(unitId);
    public UnitView? GetUnitView(Vector2I coords) => _unitViews.GetValueOrDefault(_unitMaps.GetValueOrDefault(coords));
    public void FreeUnitView(long unitId)
    {
        if (!_unitViews.TryGetValue(unitId, out var render)) return;
        render.QueueFree();
        _unitViews.Remove(unitId);
        _unitMaps.Remove(render.RealCoordsData);
    }
    public void SetUnitRealCoords(long unitId, Vector2I coords)
    {
        if (!_unitViews.TryGetValue(unitId, out var render)) return;
        _unitMaps.Remove(render.RealCoordsData);
        _unitMaps[coords] = unitId;
        render.RealCoordsData = coords;
    }
    public UnitView? CreateUnitView(Vector2I coords, IReadOnlyUnitState unit)
    {
        if (unit.IsNull()) return null;
        var unitTeamType = GameView.GetTeamType(unit.Team);
        if (!_unitViews.TryGetValue(unit.Id, out var render))
        {
            render = _unitViews[unit.Id] = GameView.Pools.UnitViewPool.Get();
            render.Init(GameView.GameStyle, unitTeamType, unit.Id, this);
            _unitMaps.Add(coords, unit.Id);
        }
        //设置单位位置和动画
        render.RealCoordsData = coords;
        render.UnitName = unit.Name;
        //血条
        render.HealthBar.MaxHealth = unit.MaxHealth;
        render.HealthBar.CurrentHealth = unit.Health;
        //护盾条
        render.HealthBar.CurrentShield = unit.Shield;
        render.OutlineColor = GameView.GameStyle[unitTeamType].OutlineColor;
        render.ResetToWishCoords();
        //Buff
        foreach (var buff in unit.Buffs)
        {
            render.BuffView.AddVisibleBuff(buff.BuffName);
        }
        return render;
    }
}