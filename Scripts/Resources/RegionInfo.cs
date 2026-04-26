using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool]
public partial class RegionInfo : Resource, IInfo<RegionNameEnum>
{
    public static readonly RegionInfo Null = new RegionInfo()
    {
        Name = RegionNameEnum.None,
        Description = "空占位区域"
    };
    [Export] public RegionNameEnum Name { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public bool IsOnece { get; set; }
    [Export] public int DefaultDuration { get; set; }
    [Export] public string Description { get; set; } = string.Empty;
}