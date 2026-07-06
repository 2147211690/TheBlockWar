using YARD;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass]
public partial class RegionInitInfo : Resource
{
    [Export(RegistryPropertyHint.PROPERTY_HINT_CUSTOM, "res://Resources/Registrys/Regions.tres")]
    public string RegionName { get; set; } = "None";
    [Export] public int Duration { get; set; } = -1;
    [Export] public string Data { get; set; } = string.Empty;
}