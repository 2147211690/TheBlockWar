using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts;

public partial class AutoLoad : Node
{
    public override void _Ready()
    {
        InfoExtensions.InfoManager = ResourceLoader.Load<InfoManager>("uid://cidx5e6oa1bc8");
        Registerer.Register(InfoExtensions.InfoManager);
    }
}