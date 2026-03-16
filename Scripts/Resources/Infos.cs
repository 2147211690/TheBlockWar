using Godot;
using Godot.Collections;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class Infos : Resource
{
    [Export] public Dictionary<BuffTypeEnum, BuffInfo> BuffInfos { get; set; } = new();
}