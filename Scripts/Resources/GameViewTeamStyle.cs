namespace TheBlockWar.Scripts.Resources;

[GlobalClass]
public partial class GameViewTeamStyle : Resource
{
    [Export] public Font Font { get; set; } = null!;
    [ExportGroup("Events")]
    [Export] public Texture2D PlaceDownTexture { get; set; } = null!;
    [Export] public Texture2D[] PathTextures { get; set; } = null!;
    [Export] public Texture2D[] PushTextures { get; set; } = null!;
    [Export] public Texture2D[] BlockNumberTextures { get; set; } = null!;
    [Export] public Texture2D DeathTexture { get; set; } = null!;
    [Export] public Texture2D LinePointTexture { get; set; } = null!;
    [Export] public Texture2D PlaceUnitTexture { get; set; } = null!;
    [Export] public float WaitAlpha { get; set; } = 0.3f;
    [ExportGroup("Units")]
    [Export] public Color OutlineColor { get; set; } = new Color("#3aa200");         // 绿色 - 单位轮廓线
    [Export] public Color HealthCurrentColor { get; set; } = new Color("#e74c3c");      // 鲜红 - 当前生命
    [Export] public Color ShieldCurrentColor { get; set; } = new Color("#3498db");     // 亮蓝 - 当前护盾
    [Export] public Color HealthBarColor { get; set; } = new Color("#2c3e50");         // 深蓝灰 - 生命条背景
    [Export] public Color ShieldBarColor { get; set; } = new Color("#1a252f");         // 更深蓝灰 - 护盾条背景
    [Export] public Color SplitColor { get; set; } = new Color("#000000", 0.4f);       // 半透明黑 - 分割线
    [ExportGroup("Effects")]
    [Export] public EffectTable EffectTable { get; set; } = null!;
    [ExportGroup("Regions")]
    [Export] public Texture2D SkillRegionTexture { get; set; } = null!;
    [Export] public Color SkillRegionColor { get; set; } = new Color("#3aa200");
    [Export] public Color PreviewRegionColor { get; set; } = new Color("#3aa200");
    public Texture2D? GetBlockNumberTexture(int number)
    {
        if (number < 0 || number >= BlockNumberTextures.Length) return null;
        return BlockNumberTextures[number];
    }
    
}