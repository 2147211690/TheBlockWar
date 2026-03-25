namespace TheBlockWar.View.Uis;
[GlobalClass]
public partial class TooltipManager : Control
{
    public event HoverChangeEvent? OnHoverChange;
    public delegate void HoverChangeEvent(object sender, object? hoverObject, bool isHover);
    public object? HoverObject { get; private set; }
    public float TooltipDelay { get; set; } = 0.5f;
    private ulong _mouseEnterTick;
    private bool _isHover = false;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (HoverObject is null || _isHover) return;
        if (Time.GetTicksMsec() - _mouseEnterTick < TooltipDelay * 1000) return;
        OnHoverChange?.Invoke(this, HoverObject, true);
        _isHover = true;
    }
    public void MouseEnter(object hoverObject)
    {
        _mouseEnterTick = Time.GetTicksMsec();
        HoverObject = hoverObject;
        _isHover = false;
    }
    public void MouseExit(object hoverObject)
    {
        OnHoverChange?.Invoke(this, HoverObject, false);
        _isHover = false;
        HoverObject = null;
    }
}
