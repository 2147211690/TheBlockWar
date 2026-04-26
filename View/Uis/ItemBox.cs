namespace TheBlockWar.View.Uis;

public partial class ItemBox : PanelContainer
{
    [Export] private TextureRect _textureRect = null!;
    private Tween? _currentTween;
    [Export] public bool CanClick { get; set; } = true;
    public Texture2D Texture
    {
        get => _textureRect.Texture;
        set => _textureRect.Texture = value;
    }
    public object? MetaData { get; set; }
    public static float MouseEnterScale = 1.04f;
    public static float AnimationDuration = 0.05f;
    public static float ClickScale = 0.96f;
    public static float SelectedScale = 1.08f;
    public bool IsSelected
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            if (value)
            {
                ApplySelectedState();
            }
            else
            {
                ApplyNormalState();
            }
        }
    }

    private void ApplySelectedState()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "scale", Vector2.One * SelectedScale, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", new Color(1.3f, 1.3f, 1.3f), AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        
        ThemeTypeVariation = "ItemBox_Selected";
    }

    private void ApplyNormalState()
    {
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "scale", Vector2.One, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", Colors.White, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        
        ThemeTypeVariation = "ItemBox_Normal";
    }

    public override void _Ready()
    {
        ApplyNormalState();

        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }
    
    public override void _GuiInput(InputEvent inputEvent)
    {
        if (!CanClick) return;
        if (inputEvent is InputEventMouseButton {Pressed:true, ButtonMask: MouseButtonMask.Left})
        {
            OnClick();
        }
    }

    private void OnClick()
    {
        if (IsSelected) return;

        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "scale", Vector2.One * ClickScale, AnimationDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.Chain().TweenProperty(this, "scale", Vector2.One * MouseEnterScale, AnimationDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnMouseExited()
    {
        if (IsSelected) return;

        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "scale", Vector2.One, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", Colors.White, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnMouseEntered()
    {
        if (IsSelected) return;

        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "scale", Vector2.One * MouseEnterScale, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", new Color(1.2f, 1.2f, 1.2f), AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}