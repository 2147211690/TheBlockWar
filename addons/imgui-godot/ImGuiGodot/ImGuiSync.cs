#if GODOT_PC
using System;
using System.Runtime.InteropServices;
using Godot;
using ImGuiNET;

namespace TheBlockWar.addons.imgui_godot.ImGuiGodot;

public partial class ImGuiSync : GodotObject
{
    public static readonly StringName GetImGuiPtrs = "GetImGuiPtrs";

    public static void SyncPtrs()
    {
        GodotObject gd = Engine.GetSingleton("ImGuiGD");
        long[] ptrs = (long[])gd.Call(GetImGuiPtrs,
            ImGui.GetVersion(),
            Marshal.SizeOf<ImGuiIO>(),
            Marshal.SizeOf<ImDrawVert>(),
            sizeof(ushort),
            sizeof(ushort)
            );

        if (ptrs.Length != 3)
        {
            throw new NotSupportedException("ImGui version mismatch");
        }

        checked
        {
            ImGui.SetCurrentContext((IntPtr)ptrs[0]);
            ImGui.SetAllocatorFunctions((IntPtr)ptrs[1], (IntPtr)ptrs[2]);
        }
    }
}
#endif
