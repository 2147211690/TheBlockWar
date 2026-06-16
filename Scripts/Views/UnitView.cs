using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

public partial class UnitView : ViewNode2D
{
    [Export] public Node2D Center { get; set; } = null!;
    [Export] public HealthBar HealthBar { get; set; } = null!;
    [Export] public BuffView BuffView { get; set; } = null!;
    [Export] public AnimatedSprite2D MainSprite { get; set; } = null!;
    [Export] public bool DrawDebug { get; set; }
    [ExportGroup("PlaceDown")]
    [Export] public float DropHeight { get; set; } = 50f;
    [Export] public float DropDuration { get; set; } = 0.1f;
    [Export] public float ImpactSquashY { get; set; } = 1.3f;
    [Export] public float ImpactStretchX { get; set; } = 0.25f;
    [Export] public float RecoveryDuration { get; set; } = 0.04f;
    public Vector2 Offset { get => Center.Position; set => Center.Position = value; }
    public IReadOnlyList<(Vector2I coords, ViewSprite2D sprite)> Fakes => _fakes ?? [];
    public Vector2I? AimCoords { get; set; }
    public Vector2I RealCoords { get; set; }
    public TeamTypeEnum TeamType { get; set; }
    public UnitsView UnitsView { get; set; } = null!;
    public long Id { get; set; }
    public Vector2I WishCoords => AimCoords ?? RealCoords;
    public GameView GameView { get; set; } = null!;

    public bool IsFocused
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _shaderMaterial?.SetShaderParameter("outline_color", OutlineColor with { A = IsFocused ? 1f : 0.5f });
        }
    }

    public bool IsDead
    {
        get;
        set
        {
            if (!value) return;
            MainSprite.Play("dead");
            HealthBar.IsDead = true;
            BuffView.ClearVisibleBuffs();
            MainSprite.Modulate = new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private List<(Vector2I coords, ViewSprite2D sprite)>? _fakes;
    private ShaderMaterial? _shaderMaterial = null!;
    private UnitInfo? _unitInfo;
    private string _unitName = string.Empty;

    public Color OutlineColor
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _shaderMaterial?.SetShaderParameter("outline_color", value with { A = IsFocused ? 1f : 0.5f });
        }
    }
    public Color WaterColor
    {
        get => _shaderMaterial?.GetShaderParameter("water_color").AsColor() ?? Colors.White;
        set => _shaderMaterial?.SetShaderParameter("water_color", value);
    }
    public float WaterHeight
    {
        get => (float)(_shaderMaterial?.GetShaderParameter("water_height").AsDouble() ?? 0);
        set => _shaderMaterial?.SetShaderParameter("water_height", value);
    }
    public string UnitName
    {
        get => _unitName;
        set
        {
            _unitInfo = value.GetUnitInfo();
            MainSprite.SpriteFrames = _unitInfo.ViewFrames;
            MainSprite.Play("idle");
            MainSprite.Offset = _unitInfo.ViewOffset;
            _unitName = value;
        }
    }

    public void Init(GameView gameView, GameViewStyle style, TeamTypeEnum teamType, long id, UnitsView unitsView)
    {
        Id = id;
        GameView = gameView;
        UnitsView = unitsView;
        TeamType = teamType;
        HealthBar.Init(gameView, style, teamType);
        BuffView.Init(gameView, style, teamType);
    }
    public override void _Ready()
    {
        _shaderMaterial = MainSprite.Material as ShaderMaterial;
        MainSprite.FrameChanged += MainSpriteOnFrameChanged;
        if (DrawDebug)
        {
            var l = new Label()
            {
                Name = "DebugLabel",
                Text = Id.ToString(),
                ZIndex = 100,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Scale = new Vector2(0.5f, 0.5f),
                Modulate = Colors.DarkMagenta,
            };
            l.LabelSettings = new LabelSettings()
            {
                LineSpacing = -18,
                FontSize = 18,
            };
            Center.AddChild(l);
        }
        
    }
    private void UpdateDebugLabel()
    {
        if (DrawDebug)
        {
            var l = Center.GetNode<Label>("DebugLabel");
            l.Text =
                $"""
                 Name:{UnitName}
                 Id:{Id}
                 Coords:{WishCoords}
                 """;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateDebugLabel();
    }

    private void MainSpriteOnFrameChanged()
    {
        if (_fakes is null) return;
        foreach (var f in _fakes)
        {
            f.sprite.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
        }
    }

    public Sprite2D AddFack(Vector2I coords)
    {
        _fakes ??= new();
        
        var fake = GameView.ViewPools.Sprite2DPool.Get();
        fake.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
        fake.Offset = _unitInfo?.ViewOffset ?? Vector2.Zero;
        fake.Modulate = new Color(1, 1, 1, 0.3f);
        fake.GamePosition = GameView.TilesView.GetGamePositionOnGround(coords);
        _fakes.Add((coords, fake));
        return fake;
    }

    public void ClearFacks()
    {
        if (_fakes is null) return;
        foreach (var fakePair in _fakes)
        {
            PushFackSprite(fakePair);
        }
        _fakes.Clear();
    }
    
    public void RemoveFack(Vector2I coords)
    {
        if (_fakes is null) return;
        var index = _fakes.FindIndex(x => x.coords == coords);
        if (index == -1) return;
        var fake = _fakes[index];
        _fakes.RemoveAt(index);
        PushFackSprite(fake);
    }
    
    private void PushFackSprite(in (Vector2I coords, ViewSprite2D sprite) fakePair)
    {
        var sprite = fakePair.sprite;
        sprite.Texture = null;
        sprite.Offset = Vector2.Zero;
        sprite.Modulate = Colors.White;
        GameView.ViewPools.Sprite2DPool.Return(sprite);
    }

    public GDTask PlayHurtAnim()
    {
        var tween = CreateTween();
        var originPos = Position;
        var originColor = Modulate;
        tween.SetParallel();
        for (int i = 0; i < 4; i++)
        {
            var r = GD.Randf() * Mathf.Pi * 2;
            var p = originPos + Vector2.Right.Rotated(r) * 5;
            tween.TweenProperty(this, "position", p, 0)
                .SetTrans(Tween.TransitionType.Linear)
                .SetDelay(i * 0.03f);
        }
        // 闪白 x3
        for (int i = 0; i < 3; i++)
        {
            tween.TweenProperty(this, "modulate", new Color(3f, 3f, 3f, 1f), 0.02f)
                .SetTrans(Tween.TransitionType.Linear)
                .SetDelay(i * 0.03f);
            tween.TweenProperty(this, "modulate", originColor, 0.02f)
                .SetTrans(Tween.TransitionType.Linear);
        }

        // 结束恢复
        return tween.WaitForTween(t =>
        {
            Position = originPos;
            Modulate = originColor;
        });
    }

    public GDTask PlayPlaceDownAnim()
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Quad);
        tween.SetEase(Tween.EaseType.In);
        
        // 起始位置：上方
        MainSprite.Position = new Vector2(0, -DropHeight);
        MainSprite.Scale = Vector2.One;
        
        // 阶段1：重型坠落（加速下落）
        tween.TweenProperty(MainSprite, "position", new Vector2(0, 10), DropDuration)
            .SetTrans(Tween.TransitionType.Expo)
            .SetEase(Tween.EaseType.In);
        
        // 阶段2：撞击地面（压扁效果）
        tween.Chain().TweenProperty(MainSprite, "scale", 
                new Vector2(ImpactStretchX, ImpactSquashY), 0.02f)
            .SetTrans(Tween.TransitionType.Quad)
            .SetEase(Tween.EaseType.Out);
        
        tween.Parallel().TweenProperty(MainSprite, "position", Vector2.Zero, 0.02f);
        
        // 阶段3：回弹恢复
        tween.Chain().TweenProperty(MainSprite, "scale", Vector2.One, RecoveryDuration)
            .SetTrans(Tween.TransitionType.Elastic)
            .SetEase(Tween.EaseType.Out);

        return tween.WaitForTween();
    }
}