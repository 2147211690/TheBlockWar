using System.Runtime.InteropServices;
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

    public TileState GetSnapshot()
    {
        return Clone();
    }

    public TileState(){}
    public TileState(TileStateData data)
    {
        Name = data.Name;
        _regions = data.Regions.CloneToListOrNullWhenCountZero();
    }
    public int FindRegion(string regionName)
    {
        return _regions?.FindIndex(a => a.RegionName == regionName) ?? -1;
    }
    /// <summary>
    /// 添加一个区域效果, 若已存在则累加持续时间
    /// </summary>
    public int AddRegion(string regionName, int duration, string? data = null)
    {
        if (duration < 0) duration = int.MaxValue;
        var list = GetRegionList();
        var index = FindRegion(regionName);
        var span = CollectionsMarshal.AsSpan(_regions);
        _isRegionStaticDirty = true;
        if (index >= 0)
        {
            span[index].Duration = duration == int.MaxValue ? int.MaxValue : span[index].Duration + duration;
            if (data != null) span[index].RegionData = data;
            return index;
        }
        else
        {
            var p = regionName.GetRegionInfo().Priority;
            var i = 0;
            for (; i < span.Length; i++)
                if (span[i].RegionName.GetRegionInfo().Priority > p) break;
            list.Insert(i, new RegionState()
            {
                RegionName = regionName,
                Duration = duration,
                RegionData = data ?? string.Empty,
            });
            return i;
        }
    }

    public int RemoveRegion(string regionName, int duration)
    {
        var index = FindRegion(regionName);
        if (index < 0) return -1;
        return RemoveRegion(index, duration);
    }

    public int ChangeRegionData(string regionName, string? regionData)
    {
        var index = FindRegion(regionName);
        if (index < 0) return -1;
        var span = CollectionsMarshal.AsSpan(_regions);
        span[index].RegionData = regionData ?? string.Empty;
        _isRegionStaticDirty = true;
        return index;
    }
    public int ChangeRegionData(int index, string? regionData)
    {
        if (index < 0 || index >= Regions.Count) return -1;
        var span = CollectionsMarshal.AsSpan(_regions);
        span[index].RegionData = regionData ?? string.Empty;
        _isRegionStaticDirty = true;
        return index;
    }
    public int RemoveRegion(int index, int duration)
    {
        if (index < 0 || index >= Regions.Count) return -1;
        var list = GetRegionList();
        var span = CollectionsMarshal.AsSpan(_regions);
        span[index].Duration = Mathf.Max(0, span[index].Duration - (duration < 0 ? int.MaxValue : duration));
        _isRegionStaticDirty = true;
        if (span[index].Duration == 0)
        {
            list.RemoveAt(index);
            return -1;
        }
        else
        {
            return index;
        }
    }

    public void ClearRegion()
    {
        GetRegionList().Clear();
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