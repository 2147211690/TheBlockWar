namespace TheBlockWar.Scripts.Views.Uis;

public partial class ItemBox : AspectRatioContainer
{
    [Export] public bool CanClick { get; set; } = true;

    [Export]
    public bool Disabled
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            if (value)
            {
                OffsetTransformScale = Vector2.One;
                Modulate = Colors.DarkGray;
            }
            else
            {
                Modulate = Colors.White;
            }
        }
    }
    [Export]
    public bool IsSelected
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            _backPanel.ThemeTypeVariation = value ? "ItemBox_Selected" : "ItemBox_Normal";
        }
    }

    [ExportGroup("Self")]
    [Export] private TextureRect _textureRect = null!;
    [Export] private Label _countLabel = null!;
    [Export] private PanelContainer _backPanel = null!;
    private Tween? _currentTween;
    
    public Texture2D Texture
    {
        get => _textureRect.Texture;
        set => _textureRect.Texture = value;
    }
    public int Count
    {
        get => int.Parse(_countLabel.Text);
        set => _countLabel.Text = value.ToString();
    }

    public bool HasCount
    {
        get => _countLabel.Visible;
        set => _countLabel.Visible = value;
    }
    public object? MetaData { get; set; }
    public static float MouseEnterScale = 1.04f;
    public static float AnimationDuration = 0.05f;
    public static float ClickScale = 0.96f;

    [Signal] public delegate void ClickEventHandler();

    public override void _Ready()
    {
        MouseEntered += OnMouseEntered;
        MouseExited += OnMouseExited;
    }

    public void Reset()
    {
        MetaData = null;
        IsSelected = false;
        Texture = null;
        CanClick = true;
        Count = 0;
        HasCount = false;
        Disabled = false;
        backing_Click = null;
    }
    
    public override void _GuiInput(InputEvent inputEvent)
    {
        if (!CanClick) return;
        if (inputEvent is InputEventMouseButton {Pressed:true, ButtonMask: MouseButtonMask.Left})
        {
            OnClick();
            EmitSignal(nameof(Click));
        }
    }

    private void OnClick()
    {
        if (Disabled) return;

        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "offset_transform_scale", Vector2.One * ClickScale, AnimationDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.Chain().TweenProperty(this, "offset_transform_scale", Vector2.One * MouseEnterScale, AnimationDuration * 0.5f)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnMouseExited()
    {
        if (Disabled) return;
        
        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "offset_transform_scale", Vector2.One, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", Colors.White, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }

    private void OnMouseEntered()
    {
        if (Disabled) return;

        _currentTween?.Kill();
        _currentTween = CreateTween();
        _currentTween.SetParallel(true);
        _currentTween.TweenProperty(this, "offset_transform_scale", Vector2.One * MouseEnterScale, AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
        _currentTween.TweenProperty(_textureRect, "modulate", new Color(1.2f, 1.2f, 1.2f), AnimationDuration)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}