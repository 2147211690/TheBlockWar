using TheBlockWar.Scripts.Games.Skills;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class SkillInfo : Resource, IInfo
{
    public static readonly SkillInfo Null = new();
    [Export] public string Name { get; set; } = "None";
}
