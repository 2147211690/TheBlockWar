using Godot;
using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class RegionInfo : Resource
{
    public static readonly RegionInfo Null = new RegionInfo()
    {
        RegionType = RegionTypeEnum.None,
        Description = "空占位区域"
    };
    [Export] public RegionTypeEnum RegionType { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public int DefaultDuration { get; set; }
    [Export] public string Description { get; set; } = string.Empty;
}