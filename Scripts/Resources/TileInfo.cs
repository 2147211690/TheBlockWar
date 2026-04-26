using TheBlockWar.Scripts.Games.Tiles;

namespace TheBlockWar.Scripts.Resources;

[GlobalClass, Tool]
public partial class TileInfo : Resource, IInfo<TileNameEnum>
{
    [Export] public TileNameEnum Name { get; set; }
}