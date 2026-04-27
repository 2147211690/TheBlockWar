using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool]
public partial class RegionInfo : Resource, IInfo
{
    public static readonly RegionInfo Null = new RegionInfo()
    {
        Name = "None",
        Description = "空占位区域"
    };
    [Export] public string Name { get; set; } = "None";
    [Export] public string[]? EnterAddBuffs { get; set; }
    [Export] public string[]? ExitRemoveBuffs { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public bool IsOnece { get; set; }
    [Export] public int DefaultDuration { get; set; }
    [Export] public string Description { get; set; } = string.Empty;
}