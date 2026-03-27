namespace TheBlockWar.View.Uis;
[GlobalClass]
public partial class TooltipManager : Control
{
    public event HoverChangeEvent? OnHoverChange;
    public delegate void HoverChangeEvent(object sender, object? hoverObject, bool isHover);
    public float TooltipDelay { get; set; } = 0.5f;
    private WeakReference<object?> _hoverObject = new(null);
    private ulong _mouseEnterTick;
    private bool _isHover = false;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (!_hoverObject.TryGetTarget(out var hoverObject) || _isHover) return;
        if (Time.GetTicksMsec() - _mouseEnterTick < TooltipDelay * 1000) return;
        OnHoverChange?.Invoke(this, hoverObject, true);
        _isHover = true;
    }
    public void MouseEnter(object hoverObject)
    {
        _mouseEnterTick = Time.GetTicksMsec();
        _hoverObject.SetTarget(hoverObject);
        _isHover = false;
    }
    public void MouseExit(object hoverObject)
    {
        OnHoverChange?.Invoke(this, hoverObject, false);
        _isHover = false;
        _hoverObject.SetTarget(null);
    }
}
