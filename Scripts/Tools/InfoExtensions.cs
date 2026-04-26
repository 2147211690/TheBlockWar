using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Tiles;
using TheBlockWar.Scripts.Games.Units;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Tools;

public static class InfoExtensions
{
    public static InfoManager InfoManager { get; set; }
    public static BuffInfo GetInfo(this BuffNameEnum name) => InfoManager.BuffInfos[name]!;
    public static SkillInfo GetInfo(this SkillNameEnum name) => InfoManager.SkillInfos[name]!;
    public static UnitInfo GetInfo(this UnitNameEnum name) => InfoManager.UnitInfos[name]!;
    public static RegionInfo GetInfo(this RegionNameEnum name) => InfoManager.RegionInfos[name]!;
    public static TileInfo GetInfo(this TileNameEnum name) => InfoManager.TileInfos[name]!;
}