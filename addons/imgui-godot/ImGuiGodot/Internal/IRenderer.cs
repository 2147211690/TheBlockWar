#if GODOT_PC
using System;
using Godot;

namespace TheBlockWar.addons.imgui_godot.ImGuiGodot.Internal;

internal interface IRenderer : IDisposable
{
    string Name { get; }
    void InitViewport(Rid vprid);
    void CloseViewport(Rid vprid);
    void Render();
    void OnHide();
}
#endif
