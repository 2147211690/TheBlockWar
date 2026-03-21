using Godot;

namespace TheBlockWar.Scripts.Tools;

public static partial class Autoload
{
    private static readonly Node? Root = (Engine.GetMainLoop() as SceneTree)?.Root;
    public static CallHelper CallHelper => field ??= Root?.GetNodeOrNull<CallHelper>("CallHelper");
}