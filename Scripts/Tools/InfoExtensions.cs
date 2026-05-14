using NLog;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Tools;

public static class InfoExtensions
{
    public static InfoManager InfoManager { get; set; } = null!;
    public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    public static BuffInfo GetBuffInfo(this string name)
    {
        if (InfoManager.BuffInfos.TryGetValue(name, out var info) && info != null) return info;
        Logger.Info($"BuffInfo {name} 没有找到, 使用空");
        return BuffInfo.Null;
    }

    public static SkillInfo GetSkillInfo(this string name)
    {
        if (InfoManager.SkillInfos.TryGetValue(name, out var info) && info != null) return info;
        Logger.Info($"SkillInfo {name} 没有找到, 使用空");
        return SkillInfo.Null;
    }
    public static UnitInfo GetUnitInfo(this string name)
    {
        if (InfoManager.UnitInfos.TryGetValue(name, out var info) && info != null) return info;
        Logger.Info($"UnitInfo {name} 没有找到, 使用空");
        return UnitInfo.Null;
    }

    public static RegionInfo GetRegionInfo(this string name)
    {
        if (InfoManager.RegionInfos.TryGetValue(name, out var info) && info != null) return info;
        Logger.Info($"RegionInfo {name} 没有找到, 使用空");
        return RegionInfo.Null;
    }

    public static TileInfo GetTileInfo(this string name)
    {
        if (InfoManager.TileInfos.TryGetValue(name, out var info) && info != null) return info;
        Logger.Info($"TileInfo {name} 没有找到, 使用空");
        return TileInfo.Null;
    }
}