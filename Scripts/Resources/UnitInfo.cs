using TheBlockWar.Scripts.Games.Units;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class UnitInfo : Resource, IInfo<UnitNameEnum>
{
    [Export] public UnitNameEnum Name { get; set; }
}