using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Regions;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Tiles;
using TheBlockWar.Scripts.Games.Units;
using System.Linq;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class InfoManager : Resource
{
    [Export] public Godot.Collections.Dictionary<string, BuffInfo?> BuffInfos { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, RegionInfo?> RegionInfos { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, UnitInfo?> UnitInfos { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, TileInfo?> TileInfos { get; set; } = new();
    [Export] public Godot.Collections.Dictionary<string, SkillInfo?> SkillInfos { get; set; } = new();
    
    #if TOOLS
    [ExportGroup("Tools")]
    [Export(PropertyHint.Dir)] public string BuffInfoPath { get; set; } = string.Empty;
    [ExportToolButton("读取创建BuffInfos")]
    public Callable AutoCreateBuffInfosButton => Callable.From(OnAutoCreateBuffInfos);
    [Export(PropertyHint.Dir)] public string RegionInfoPath { get; set; } = string.Empty;
    [ExportToolButton("读取创建RegionInfos")]
    public Callable AutoCreateRegionInfosButton => Callable.From(OnAutoCreateRegionInfos);
    [Export(PropertyHint.Dir)] public string UnitInfoPath { get; set; } = string.Empty;
    [ExportToolButton("读取创建UnitInfos")]
    public Callable AutoCreateUnitInfosButton => Callable.From(OnAutoCreateUnitInfos);
    [Export(PropertyHint.Dir)] public string TileInfoPath { get; set; } = string.Empty;
    [ExportToolButton("读取创建TileInfos")]
    public Callable AutoCreateTileInfosButton => Callable.From(OnAutoCreateTileInfos);
    [Export(PropertyHint.Dir)] public string SkillInfoPath { get; set; } = string.Empty;
    [ExportToolButton("读取创建SkillInfos")]
    public Callable AutoCreateSkillInfosButton => Callable.From(OnAutoCreateSkillInfos);
    [ExportToolButton("读取创建所有Infos")]
    public Callable AutoCreateAllInfosButton => Callable.From(OnAutoCreateAllInfos);
    
    private void OnAutoCreateBuffInfos()
    {
        AutoCreateInfos(
            BuffInfos,
            () => new BuffInfo(),
            BuffInfoPath,
            "BuffInfo",
            Enum.GetValuesAsUnderlyingType<BuffNameEnum>().Cast<BuffNameEnum>().Select(a => a.ToString()).ToArray()
        );
    }
    private void OnAutoCreateRegionInfos()
    {
        AutoCreateInfos(
            RegionInfos, 
            () => new RegionInfo(),
            RegionInfoPath, 
            "RegionInfo",
            Enum.GetValuesAsUnderlyingType<RegionNameEnum>().Cast<RegionNameEnum>().Select(a => a.ToString()).ToArray()
        );
    }

    private void OnAutoCreateUnitInfos()
    {
        AutoCreateInfos(
            UnitInfos, 
            () => new UnitInfo(),
            UnitInfoPath, 
            "UnitInfo",
            Enum.GetValuesAsUnderlyingType<UnitNameEnum>().Cast<UnitNameEnum>().Select(a => a.ToString()).ToArray()
        );
    }
    private void OnAutoCreateTileInfos()
    {
        AutoCreateInfos(
            TileInfos, 
            () => new TileInfo(),
            TileInfoPath, 
            "TileInfo",
            Enum.GetValuesAsUnderlyingType<TileNameEnum>().Cast<TileNameEnum>().Select(a => a.ToString()).ToArray()
        );
    }

    private void OnAutoCreateSkillInfos()
    {
        AutoCreateInfos(
            SkillInfos, 
            () => new SkillInfo(),
            SkillInfoPath, 
            "SkillInfo",
            Enum.GetValuesAsUnderlyingType<SkillNameEnum>().Cast<SkillNameEnum>().Select(a => a.ToString()).ToArray()
        );
    }

    private void OnAutoCreateAllInfos()
    {
        OnAutoCreateBuffInfos();
        OnAutoCreateRegionInfos();
        OnAutoCreateUnitInfos();
        OnAutoCreateTileInfos();
        OnAutoCreateSkillInfos();
    }
    private void AutoCreateInfos<TValue>(
        IDictionary<string, TValue?> dict,
        Func<TValue> resourceFactory,
        string infoDir,
        string namedSuffix,
        string[] names) where TValue : Resource, IInfo
    {
        dict.Clear();
        var hasCreated = new Dictionary<string, string>();
        foreach (var name in DirAccess.GetFilesAt(infoDir))
        {
            var path = $"{infoDir}/{name}";
            TValue r;
            try
            {
                r = ResourceLoader.Load<TValue>(path);
            }
            catch (Exception e)
            {
                continue;
            } 
            GD.Print($"读取到现有{path}");
            var type = r.Name;
            if (!hasCreated.TryAdd(type, path))
            {
                GD.PushWarning($"发现重复类型{type} {path}");
            }
        }
        var isRenamed = false;
        foreach (var name in names)
        {
            var path = $"{infoDir}/{name}{namedSuffix}.tres";
            if (hasCreated.TryGetValue(name, out var oldPath) && oldPath != path)
            {
                DirAccess.RenameAbsolute(oldPath, path);
                isRenamed = true;
                GD.Print($"重命名{oldPath}为{path}");
            }
            if (!FileAccess.FileExists(path))
            {
                var info = resourceFactory();
                info.Name = name;
                ResourceSaver.Save(info, path);
                GD.Print($"创建{path}");
                dict[name] = ResourceLoader.Load<TValue>(path);
            }
            else
            {
                var info = dict[name] = ResourceLoader.Load<TValue>(path);
                info.Name = name;
            }
        }
        if (isRenamed)
        {
            var fs = EditorInterface.Singleton.GetResourceFilesystem();
            fs.Scan();
        }
        EmitChanged();
    }
    #endif
}