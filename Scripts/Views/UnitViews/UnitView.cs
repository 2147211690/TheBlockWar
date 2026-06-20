using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views.UnitViews;
[Icon("res://addons/at-icons/node2d/tank.svg")]
public partial class UnitView : ViewNode2D
{
    [Export] public bool DrawDebug { get; set; }
    [ExportGroup("Self")]
    [Export] public Node2D Center { get; set; } = null!;
    [Export] public HealthBar HealthBar { get; set; } = null!;
    [Export] public BuffView BuffView { get; set; } = null!;
    [Export] public AnimatedSprite2D MainSprite { get; set; } = null!;
    public IReadOnlyList<(Vector2I coords, ViewSprite2D sprite)> Fakes => _fakes ?? [];

    public Vector2I? EventAimCoords
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            ResetToWishCoords();
        }
    }

    public Vector2I RealCoordsData
    {
        get => field;
        set
        {
            if (field == value) return;
            field = value;
            ResetToWishCoords();
        }
    }

    public TeamTypeEnum TeamTypeData { get; set; }
    public UnitsView UnitsView { get; set; } = null!;
    public long IdData { get; set; }
    public Vector2I WishCoords => EventAimCoords ?? RealCoordsData;
    public IGameView GameView => UnitsView.GameView;

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

    public void Init(GameViewStyle style, TeamTypeEnum teamType, long id, UnitsView unitsView)
    {
        IdData = id;
        UnitsView = unitsView;
        TeamTypeData = teamType;
        HealthBar.Init(style[teamType]);
        BuffView.Init(style[teamType]);
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
                Text = IdData.ToString(),
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
        void MainSpriteOnFrameChanged()
        {
            if (_fakes is null) return;
            foreach (var f in _fakes)
            {
                f.sprite.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
            }
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
                 Id:{IdData}
                 Coords:{WishCoords}
                 """;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateDebugLabel();
    }
    

    /// <summary>
    /// 使用WishCoords更新GamePosition
    /// </summary>
    public void UpdateGamePosition()
    {
        GamePosition = GameView.CoordsToGamePosition(WishCoords);
    }
    
    public void EventFack(Vector2I coords)
    {
        _fakes ??= new();
        
        var fake = GameView.Pools.Sprite2DPool.Get();
        fake.Texture = MainSprite.SpriteFrames.GetFrameTexture(MainSprite.Animation, MainSprite.Frame);
        fake.Offset = _unitInfo?.ViewOffset ?? Vector2.Zero;
        fake.Modulate = new Color(1, 1, 1, 0.3f);
        fake.GamePosition = GameView.CoordsToGamePosition(coords);
        _fakes.Add((coords, fake));
    }

    private void ClearFacks()
    {
        if (_fakes is null) return;
        foreach (var fakePair in _fakes)
        {
            GameView.Pools.Sprite2DPool.Return(fakePair.sprite);
        }
        _fakes.Clear();
    }

    public void ResetToWishCoords()
    {
        GamePosition = GameView.CoordsToGamePosition(WishCoords);
        var tInfo = GameView.GetTileInfo(WishCoords);
        WaterColor = tInfo.DominantColor;
        WaterHeight = tInfo.WaterHeight;
    }

    public void EventClear()
    {
        ClearFacks();
        EventAimCoords = null;
        BuffView.EventClear();
        HealthBar.EventClear();
    }
}