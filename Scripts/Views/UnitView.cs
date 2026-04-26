using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;

public partial class UnitView : Node2D
{
    [Export] public Godot.Collections.Dictionary<string, Vector2> Offsets = new();
    [Export] public AnimatedSprite2D MainSprite { get; set; } = null!;
    [Export] public Node2D Center { get; set; } = null!;
    [Export] public HealthBar HealthBar { get; set; } = null!;
    [Export] public BuffView BuffView { get; set; } = null!;
    [ExportGroup("PlaceDown")]
    [Export] public float DropHeight { get; set; } = 50f;
    [Export] public float DropDuration { get; set; } = 0.1f;
    [Export] public float ImpactSquashY { get; set; } = 1.3f;
    [Export] public float ImpactStretchX { get; set; } = 0.25f;
    [Export] public float RecoveryDuration { get; set; } = 0.04f;
    public Vector2 Offset { get => Center.Position; set => Center.Position = value; }
    public IReadOnlyList<(Vector2I coords, Sprite2D sprite)> Fakes => _fakes ?? [];
    public Vector2I? AimCoords { get; set; }
    public Vector2I RealCoords { get; set; }
    public TeamTypeEnum TeamType { get; set; }
    public UnitsView UnitsView { get; set; } = null!;
    public uint Id { get; set; }
    public Vector2I WishCoords => AimCoords ?? RealCoords;
    public GameView GameView { get; set; } = null!;
    private List<(Vector2I coords, Sprite2D sprite)>? _fakes;
    private ShaderMaterial? _shaderMaterial = null!;
    public Color OutlineColor
    {
        get => _shaderMaterial?.GetShaderParameter("outline1_color").AsColor() ?? Colors.White;
        set => _shaderMaterial?.SetShaderParameter("outline1_color", value);
    }

    public string UnitName
    {
        get;
        set
        {
            MainSprite.Play(value);
            MainSprite.Offset = Offsets.GetValueOrDefault(value, Vector2.Zero);
            field = value;
        }
    } = string.Empty;

    public void Init(GameView gameView, GameViewStyle style, TeamTypeEnum teamType, UnitsView unitsView)
    {
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
    }

    private void MainSpriteOnFrameChanged()
    {
        if (_fakes is null) return;
        foreach (var f in _fakes)
        {
            f.sprite.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
        }
    }

    public Sprite2D AddFack(Vector2I coords, TilesView tilesView)
    {
        _fakes ??= new();
        
        var fake = GameView.ViewPools.Sprite2DPool.Pop();
        fake.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
        fake.Offset = tilesView.GetRenderOffset(coords, Layer.Underground, 0) + Offsets.GetValueOrDefault(UnitName);
        fake.Modulate = new Color(1, 1, 1, 0.3f);
        fake.Position = tilesView.GetRenderPosition(coords, Layer.Underground, 0);
        
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
    
    private void PushFackSprite(in (Vector2I coords, Sprite2D sprite) fakePair)
    {
        var sprite = fakePair.sprite;
        sprite.Texture = null;
        sprite.Offset = Vector2.Zero;
        sprite.Modulate = Colors.White;
        GameView.ViewPools.Sprite2DPool.Push(sprite);
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