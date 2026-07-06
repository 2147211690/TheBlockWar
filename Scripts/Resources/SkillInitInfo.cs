using YARD;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool]
public partial class SkillInitInfo : Resource
{
    [Export(RegistryPropertyHint.PROPERTY_HINT_CUSTOM, "res://Resources/Registrys/Skills.tres")]
    public string SkillName { get; set; } = "None";
}