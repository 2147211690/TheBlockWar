using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;
[GlobalClass]
public partial class DrawNode : ViewNode2D
{
    public Action<DrawNode, bool>? DrawAction
    {
        get => field;
        set
        {
            if (_hadDrawn) DrawAction?.Invoke(this, false);
            field = value;
            if (IsVisibleInTree()) QueueRedraw();
        }
    }

    public bool ProcessRedraw { get; set; }
    public float Time => (float)Godot.Time.GetTicksMsec() / 1000 - ResetTime;
    public float DeltaTime { get; private set; }
    public float ResetTime { get; private set; }
    public object? Args { get; set; }
    private bool _hadDrawn = false;
    public void Reset()
    {
        DeltaTime = 0;
        ResetTime = 0;
        _hadDrawn = false;
        Args = null;
    }

    public DrawNode()
    {
        VisibilityChanged += OnVisibilityChanged;
    }

    private void OnVisibilityChanged()
    {
        if (DrawAction == null) return;
        if (!IsVisibleInTree() && _hadDrawn)
        {
            _hadDrawn = false;
            DrawAction?.Invoke(this, false);
        }
    }

    public override void _Process(double delta)
    {
        if (DrawAction == null) return;
        DeltaTime = (float)delta;
        if (ProcessRedraw) QueueRedraw();
    }

    public override void _Draw()
    {
        base._Draw();
        if (DrawAction == null) return;
        if (_hadDrawn) DrawAction?.Invoke(this, false);
        _hadDrawn = true;
        DrawAction?.Invoke(this, true);
    }
    
}