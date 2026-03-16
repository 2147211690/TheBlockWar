using Godot;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class BuffInfo : Resource
{
    [Export] public BuffTypeEnum BuffType { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public string Description { get; set; } = string.Empty;
}