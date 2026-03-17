using System;
using System.Collections.Generic;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TileState : IReadOnlyTileState
{
    public static readonly TileState Empty = new() { Tag = TileTag.Empty };
    public TileTag Tag { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsOccupied { get; set; }
    public IReadOnlyList<RegionState> Regions => _regions ?? [];
    private List<RegionState>? _regions;
    
    /// <summary>
    /// 添加一个区域效果,不做检查
    /// </summary>
    public void AddRegion(RegionTypeEnum regionType, int duration)
    {
        GetRegionList().Add(new RegionState
        {
            RegionType = regionType,
            Duration = duration
        });
    }

    public void RemoveRegion(RegionTypeEnum regionType)
    {
        if (!this.TryFindRegion(regionType, out var index)) return;
        GetRegionList().RemoveAt(index);
    }
    public void RemoveRegion(int index)
    {
        if (index < 0 || index >= Regions.Count) return;
        GetRegionList().RemoveAt(index);
    }

    public void ModifyRegion(RegionTypeEnum regionType, int modifyDuration)
    {
        if (!this.TryFindRegion(regionType, out var index)) return;
        GetRegionList()[index] = new RegionState
        {
            RegionType = regionType,
            Duration = modifyDuration
        };
    }
    public void ModifyRegion(int index, int modifyDuration)
    {
        if (index < 0 || index >= Regions.Count) return;
        GetRegionList()[index] = new RegionState
        {
            RegionType = Regions[index].RegionType,
            Duration = modifyDuration
        };
    }
    public void ClearRegions()
    {
        _regions?.Clear();
    }
    
    private List<RegionState> GetRegionList() => _regions ??= [];
    
    public TileState Clone()
    {
        return CopyTo(new TileState());
    }
    public TileState CopyTo(TileState tile)
    {
        tile.Tag = Tag;
        tile.Name = Name;
        tile.IsOccupied = IsOccupied;
        tile._regions = _regions?.Count > 0 ? _regions.CloneToList() : null;
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