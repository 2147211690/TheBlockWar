using System;
using Godot;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class DrawNode : Node2D
{
    public Action<DrawNode>? DrawAction { get; set; }
    public bool ProcessRedraw { get; set; }
    public float Time => (float)Godot.Time.GetTicksMsec() / 1000 - ResetTime;
    public float DeltaTime { get; private set; }
    public float ResetTime { get; private set; }
    public object? Args { get; set; }
    public void Reset()
    {
        DeltaTime = 0;
        ResetTime = 0;
    }
    
    public override void _Process(double delta)
    {
        DeltaTime = (float)delta;
        if (ProcessRedraw) QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();
        DrawAction?.Invoke(this);
    }
}