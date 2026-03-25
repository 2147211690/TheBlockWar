using ImGuiNET;
using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Tests;
[SceneTree]
public partial class TestMain : Node2D
{
    public override void _Process(double delta)
    {
        base._Process(delta);
        ImGui.Begin("测试");
        if (ImGui.Button("添加Buff1"))
        {
            _.BuffView.ChangeBuffChange(BuffTypeEnum.Test, 1);
        }
        if (ImGui.Button("移除Buff1"))
        {
            _.BuffView.ChangeBuffChange(BuffTypeEnum.Test, -1);
        }
        if (ImGui.Button("添加Buff2"))
        {
            _.BuffView.ChangeBuffChange(BuffTypeEnum.None, 1);
        }
        if (ImGui.Button("移除Buff2"))
        {
            _.BuffView.ChangeBuffChange(BuffTypeEnum.None, -1);
        }

        if (ImGui.Button("切换ItemBoxIsSelected"))
        {
            _.CanvasLayer.ItemBox.IsSelected = !_.CanvasLayer.ItemBox.IsSelected;
        }

        if (ImGui.Button("添加ItemBox"))
        {
            _.CanvasLayer.CombatUi.AddSkillItem();
        }
        if (ImGui.Button("移除ItemBox"))
        {
            _.CanvasLayer.CombatUi.ClearSkillItem();
        }
        ImGui.End();
    }
}