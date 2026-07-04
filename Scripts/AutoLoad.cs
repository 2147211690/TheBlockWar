using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using YARD;

namespace TheBlockWar.Scripts;

public partial class AutoLoad : Node
{
    public override void _Ready()
    {
        var buffRegistry = 
            new Registry<BuffInfo>(ResourceLoader.Load("res://Resources/Registrys/Buffs.tres"));
        var regionRegistry = 
            new Registry<RegionInfo>(ResourceLoader.Load("res://Resources/Registrys/Regions.tres"));
        var unitRegistry = 
            new Registry<UnitInfo>(ResourceLoader.Load("res://Resources/Registrys/Units.tres"));
        var tileRegistry = 
            new Registry<TileInfo>(ResourceLoader.Load("res://Resources/Registrys/Tiles.tres"));
        var skillRegistry = 
            new Registry<SkillInfo>(ResourceLoader.Load("res://Resources/Registrys/Skills.tres"));

        InfoExtensions.InfoManager = new InfoManager(
            buffRegistry.LoadAllBlocking(),
            regionRegistry.LoadAllBlocking(),
            unitRegistry.LoadAllBlocking(),
            tileRegistry.LoadAllBlocking(),
            skillRegistry.LoadAllBlocking()
        );
        Registerer.Register(InfoExtensions.InfoManager);
    }
}