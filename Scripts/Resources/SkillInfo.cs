using TheBlockWar.Scripts.Games.Skills;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class SkillInfo : Resource, IInfo<SkillNameEnum>
{
    [Export] public SkillNameEnum Name { get; set; } = SkillNameEnum.None;
}
