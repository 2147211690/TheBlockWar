using TheBlockWar.Scripts.Games.Skills;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool, Icon("res://addons/at-icons/node/sword.svg")]
public partial class SkillInfo : Resource, IInfo
{
    public static readonly SkillInfo Null = new();
    [Export] public Texture2D Icon { get; set; } = null!;
    [Export] public string Name { get; set; } = "None";
    [Export] public string Description { get; set; } = "";
    public ISkill Instance { get => field ??= new EmptySkill(Name); set => field ??= value; }
}
