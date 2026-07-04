using YARD;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class BuffInitInfo : Resource
{
    [Export(RegistryPropertyHint.PROPERTY_HINT_CUSTOM, "res://Resources/Registrys/Buffs.tres")]
    public string BuffName { get; set; } = "None";
    [Export] public int Count { get; set; } = 1;
}