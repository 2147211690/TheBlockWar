using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Scripts.Games.Regions;

public static class Regions
{
    private static readonly Dictionary<RegionNameEnum, IRegion> _regions = new();
    public static void Register(IRegion region)
    {
        if (_regions.TryAdd(region.Name, region)) return;
        GD.PushWarning($"{region.GetType()}:另一个region已注册为{region.Name}");
    }
    public static IRegion GetInstance(this RegionNameEnum name)
    {
        if (_regions.TryGetValue(name, out var region)) return region;
        GD.PushWarning($"未注册region:{name},使用空region");
        _regions[name] = region = new EmptyRegion(name);
        return region;
    }
}
public class EmptyRegion(RegionNameEnum name) : Region(name){}
