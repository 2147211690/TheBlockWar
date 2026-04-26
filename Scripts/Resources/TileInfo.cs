using TheBlockWar.Scripts.Games.Tiles;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class TileInfo : Resource, IInfo
{
    public static readonly TileInfo Null = new();
    [Export] public string Name { get; set; } = "None";
}