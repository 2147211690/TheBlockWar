using TheBlockWar.Scripts.Games.Skills;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool, Icon("res://addons/at-icons/node/sword.svg")]
public partial class SkillInfo : Resource, IInfo
{
    public static readonly SkillInfo Null = new();
    [Export] public string Name { get; set; } = "None";
    public ISkill Instance { get => field ??= new EmptySkill(Name); set => field ??= value; }
}
