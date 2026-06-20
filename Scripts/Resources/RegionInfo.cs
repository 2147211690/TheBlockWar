using TheBlockWar.Scripts.Games.Regions;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool, Icon("res://addons/at-icons/node/vertex.svg")]
public partial class RegionInfo : Resource, IInfo
{
    public static readonly RegionInfo Null = new RegionInfo()
    {
        Name = "None",
        Description = "空占位区域"
    };
    [Export] public string Name { get; set; } = "None";
    [Export] public string[]? RegionBuffs { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public bool IsOnece { get; set; }
    [Export] public int DefaultDuration { get; set; }
    /// <summary>
    /// 优先级，越低越先执行
    /// </summary>
    [Export] public int Priority { get; set; } = 0;
    [Export] public string Description { get; set; } = string.Empty;
    public IRegion Hook { get => field ??= new EmptyRegion(Name); set => field ??= value; }
}