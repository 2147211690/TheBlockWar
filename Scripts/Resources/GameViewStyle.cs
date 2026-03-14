using Godot;
using TheBlockWar.Scripts.Games;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class GameViewStyle : Resource
{
    [Export] public GameViewTeamStyle[] TeamStyles { get; set; } = null!;

    public GameViewTeamStyle this[TeamTypeEnum teamTypeEnum] => 
        TeamStyles.Length <= (int)teamTypeEnum ? TeamStyles[0] : TeamStyles[(int)teamTypeEnum];
}