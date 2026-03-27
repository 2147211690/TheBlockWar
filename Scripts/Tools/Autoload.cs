using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Tools;

public static partial class Autoload
{
    private static readonly Node? Root = (Engine.GetMainLoop() as SceneTree)?.Root;
    public static CallHelper CallHelper => field ??= Root?.GetNodeOrNull<CallHelper>("CallHelper");
    public static Texture2D DefaultTexture => field ??= ResourceLoader.Load<Texture2D>("res://icon.svg");
    
}