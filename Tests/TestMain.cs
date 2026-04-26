using ImGuiNET;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Tests;
[SceneTree]
public partial class TestMain : Node2D
{
    private TaskQueue _taskQueue = new();
    private int animationIndex = 0;
    public override void _Ready()
    {
        base._Ready();
        TaskTracker.ShowTrackerWindow();

    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        ImGui.Begin("测试");
        if (ImGui.Button("添加Buff1"))
        {
            _.BuffView.ChangeBuffChange(BuffNames.Test, 1);
        }
        if (ImGui.Button("移除Buff1"))
        {
            _.BuffView.ChangeBuffChange(BuffNames.Test, -1);
        }
        if (ImGui.Button("添加Buff2"))
        {
            _.BuffView.ChangeBuffChange(BuffNames.None, 1);
        }
        if (ImGui.Button("移除Buff2"))
        {
            _.BuffView.ChangeBuffChange(BuffNames.None, -1);
        }

        if (ImGui.Button("切换ItemBoxIsSelected"))
        {
            _.CanvasLayer.ItemBox.IsSelected = !_.CanvasLayer.ItemBox.IsSelected;
        }
        
        if (ImGui.Button("移除ItemBox"))
        {
            _.CanvasLayer.CombatUi.ClearSkillItem();
        }
        
        ImGui.End();

        ImGui.Begin("测试动画队列");
        if (ImGui.Button("添加3s动画"))
        {
            AddAnim().Forget();
            
        }
        ImGui.End();

    }
    async GDTask AddAnim()
    {
        var index = animationIndex++;
        GD.Print($"开始添加动画{index}");
        await _taskQueue.AddTask(async () =>
        {
            await GDTask.Delay(3000);
            GD.Print($"动画{index}完成");
        });
        GD.Print($"检测到动画{index}完成");
    }
}
