using System.Linq;

namespace TheBlockWar.Scripts.Resources;

public class InfoManager
{
    public InfoManager(Godot.Collections.Dictionary<StringName, BuffInfo> buffInfos, Godot.Collections.Dictionary<StringName, RegionInfo> regionInfos, Godot.Collections.Dictionary<StringName, UnitInfo> unitInfos, Godot.Collections.Dictionary<StringName, TileInfo> tileInfos, Godot.Collections.Dictionary<StringName, SkillInfo> skillInfos)
    {
        BuffInfos = buffInfos;
        RegionInfos = regionInfos;
        UnitInfos = unitInfos;
        TileInfos = tileInfos;
        SkillInfos = skillInfos;
    }

    public Godot.Collections.Dictionary<StringName, BuffInfo> BuffInfos { get; }
    public Godot.Collections.Dictionary<StringName, RegionInfo> RegionInfos { get; }
    public Godot.Collections.Dictionary<StringName, UnitInfo> UnitInfos { get; }
    public Godot.Collections.Dictionary<StringName, TileInfo> TileInfos { get; }
    public Godot.Collections.Dictionary<StringName, SkillInfo> SkillInfos { get; }
}