using System.Runtime.CompilerServices;
using Godot;

namespace TheBlockWar.addons.imgui_godot.ImGuiGodot.Internal;

internal static class Util
{
    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    public static extern Rid ConstructRid(ulong id);
}
