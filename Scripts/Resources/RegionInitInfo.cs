namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class RegionInitInfo : Resource
{
    [Export] public string RegionName { get; set; } = "None";
    [Export] public int Duration { get; set; } = 0;
    [Export] public string Data { get; set; } = string.Empty;
}