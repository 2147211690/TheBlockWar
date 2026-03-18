using System.Collections.Generic;

namespace TheBlockWar.Scripts.Games.Regions;

public static class Regions
{
    private static readonly Dictionary<RegionTypeEnum, IRegion> _regions = new();
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public static void Register(IRegion region)
    {
        _regions[region.Info.RegionType] = region;
    }
    public static IRegion GetInstance(this RegionTypeEnum type)
    {
        return _regions[type];
    }

    static Regions()
    {
        Register(new Test1Region());
        Register(new Test2Region());
    }
}