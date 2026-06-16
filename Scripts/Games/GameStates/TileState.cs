using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TileState : IReadOnlyTileState
{
    public static readonly TileState Empty = new() {};
    public string Name { get; set; } = "Air";
    public IReadOnlyList<RegionState> Regions => _regions ?? [];
    public IReadOnlyList<RegionState> RegionsStatic
    {
        get
        {
            if (_isRegionStaticDirty)
            {
                _regionsStatic = _regions.CloneToListOrNullWhenCountZero();
                _isRegionStaticDirty = false;
            }
            return _regionsStatic ?? [];
        }
    }
    private bool _isRegionStaticDirty = true;
    private List<RegionState>? _regions;
    private List<RegionState>? _regionsStatic;

    
    public TileStateData ToData()
    {
        return new TileStateData
        {
            Name = Name,
            Regions = _regions.CloneToListOrNullWhenCountZero(),
        };
    }
    public TileState(){}
    public TileState(TileStateData data)
    {
        Name = data.Name;
        _regions = data.Regions.CloneToListOrNullWhenCountZero();
    }
    /// <summary>
    /// 添加一个区域效果,不做检查
    /// </summary>
    public void AddRegion(string regionName, int duration)
    {
        var list = GetRegionList();
        list.Add(new RegionState
        {
            RegionName = regionName,
            Duration = duration
        });
        _isRegionStaticDirty = true;
    }

    public void RemoveRegion(string regionName)
    {
        if (!this.TryFindRegion(regionName, out var index)) return;
        var list = GetRegionList();
        list.RemoveAt(index);
        _isRegionStaticDirty = true;
    }
    public void RemoveRegion(int index)
    {
        if (index < 0 || index >= Regions.Count) return;
        var list = GetRegionList();
        list.RemoveAt(index);
        _isRegionStaticDirty = true;
    }
    public void ModifyRegion(string regionName, int modifyDuration)
    {
        if (!this.TryFindRegion(regionName, out var index)) return;
        var list = GetRegionList();
        list[index] = new RegionState
        {
            RegionName = regionName,
            Duration = modifyDuration
        };
        _isRegionStaticDirty = true;
    }
    public void ModifyRegion(int index, int modifyDuration)
    {
        if (index < 0 || index >= Regions.Count) return;
        var list = GetRegionList();
        list[index] = new RegionState
        {
            RegionName = Regions[index].RegionName,
            Duration = modifyDuration
        };
        _isRegionStaticDirty = true;
    }
    public void ClearRegion()
    {
        var list = GetRegionList();
        list.Clear();
        _isRegionStaticDirty = true;
    }
    
    private List<RegionState> GetRegionList() => _regions ??= [];
    
    public TileState Clone()
    {
        return CopyTo(new TileState());
    }
    public TileState CopyTo(TileState tile)
    {
        tile.Name = Name;
        if (tile._regions is { Count: > 0 } && _regions is { Count: > 0 } && _regions.Count == tile._regions.Count)
        {
            for (int i = 0; i < _regions.Count; i++)
            {
                tile._regions[i] = _regions[i];
            }
        }
        else tile._regions = _regions.CloneToListOrNullWhenCountZero();
        tile._isRegionStaticDirty = _isRegionStaticDirty;
        tile._regionsStatic = _isRegionStaticDirty ? null : _regionsStatic;
        return tile;
    }


}
[Flags]
public enum TileTag
{
    Empty = 1,
    Water = 1 << 1,
    Ground  = 1 << 2,
}