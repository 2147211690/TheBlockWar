using Godot;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class GameViewStyle : Resource
{
    
    [ExportGroup("Events")]
    [Export] public Texture2D PlaceDownTexture { get; set; } = null!;
    [Export] public Texture2D[] PathTextures { get; set; } = null!;
}