using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Resources;

namespace TheBlockWar.Scripts.Views;

[GlobalClass]
public partial class HealthBar : Node2D
{
    [Export] public GameViewStyle Style { get; set; } = null!;
    [Export] public TeamTypeEnum TeamType { get; set; } = TeamTypeEnum.Player;
    [Export] public Vector2 HealthOffset { get; private set; } = new(0, -30);
    [Export] public Vector2 ShieldOffset { get; private set; } = new(0, -40);
    [Export] public float BlockWidth { get; private set; } = 8f; // 每格宽度
    [Export] public float Height { get; private set; } = 6;
    [Export] public float ShieldHeight { get; private set; } = 2;
    [Export] public float SplitValue { get; private set; } = 10f; // 每格代表的血量
    [Export] public float FlashFrequency { get; private set; } = 6f; // 闪烁频率
    [Export] public float FlashAmplitude { get; private set; } = 0.5f; // 闪烁强度
    [Export] public float ColorLightChange { get; private set; } = 0.4f; // 颜色变化强度
    
    public float FlashTimer => Time.GetTicksMsec() / 1000f;

    public float MaxHealth { get; set => SetFloatViewValue(ref field, value); }
    public float CurrentHealth { get; set => SetFloatViewValue(ref field, value); }
    public float HealthChange { get; set => SetFloatViewValue(ref field, value); }
    public float CurrentShield { get; set => SetFloatViewValue(ref field, value); }
    public float ShieldChange { get; set => SetFloatViewValue(ref field, value); }
    public GameView GameView { get; set; } = null!;
    private void SetFloatViewValue(ref float field, float value)
    {
        if (Math.Abs(value - field) < float.Epsilon) return;
        field = value;
    }

    public void Init(GameView gameView, GameViewStyle style, TeamTypeEnum teamType)
    {
        GameView = gameView;
        Style = style;
        TeamType = teamType;
    }
    
    public override void _Process(double delta)
    {
        QueueRedraw();
    }

    public override void _Draw()
    {
        if (MaxHealth <= 0)
            return;
        // 颜色根据是否本地玩家选择来切换
        var style = Style;
        // 总宽度根据格子数量计算
        int blockCount = Mathf.CeilToInt(MaxHealth / SplitValue);
        float totalWidth = blockCount * BlockWidth;

        float flashValue = (Mathf.Sin(FlashTimer * FlashFrequency) * 0.5f + 0.5f) * FlashAmplitude + (1 - FlashAmplitude);

        // === 绘制血条 ===
        DrawBarRect(HealthOffset, Height + 4, totalWidth + 4, 1, style[TeamType].HealthBarColor);

        float targetHealth = Mathf.Clamp(CurrentHealth + HealthChange, 0, MaxHealth);
        DrawHealthBar(HealthOffset, Height, totalWidth, CurrentHealth, targetHealth, MaxHealth, flashValue, style);

        DrawSplitLine(HealthOffset, Height, totalWidth, blockCount, style);

        // === 绘制护盾条 ===
        if (CurrentShield > 0 || ShieldChange != 0)
        {
            DrawBarRect(ShieldOffset, ShieldHeight + 2, totalWidth + 4, 1, style[TeamType].ShieldBarColor);

            float targetShield = Mathf.Max(0, CurrentShield + ShieldChange);
            DrawShieldBar(ShieldOffset, ShieldHeight, totalWidth, CurrentShield, targetShield, MaxHealth, flashValue, style);

            DrawSplitLine(ShieldOffset, ShieldHeight, totalWidth, blockCount, style);
        }
    }

    public void ClearAllChanges()
    {
        HealthChange = 0;
        ShieldChange = 0;
        QueueRedraw();
    }

    private Color GetLighterColor(in Color color, float change)
    {
        return color with { V = Mathf.Clamp(color.V * (1 + change), 0, 1) };
    }
    private void DrawHealthBar(Vector2 center, float height, float width, float cur, float target, float max, float flash, GameViewStyle style)
    {
        var curV = Mathf.Clamp(cur / max, 0, 1);
        var targetV = Mathf.Clamp(target / max, 0, 1);

        if (cur > target)
        {
            // 受伤部分闪烁
            DrawBarRect(center, height, width, curV, style[TeamType].HealthCurrentColor);
            DrawFlashingRect(center, height, width, targetV, curV, GetLighterColor(style[TeamType].HealthCurrentColor, -ColorLightChange), flash);
        }
        else if (cur < target)
        {
            // 治疗部分闪烁
            DrawBarRect(center, height, width, curV, style[TeamType].HealthCurrentColor);
            DrawFlashingRect(center, height, width, curV, targetV, GetLighterColor(style[TeamType].HealthCurrentColor, ColorLightChange), flash);
        }
        else
        {
            DrawBarRect(center, height, width, curV, style[TeamType].HealthCurrentColor);
        }
    }

    private void DrawShieldBar(Vector2 center, float height, float width, float cur, float target, float max, float flash, GameViewStyle style)
    {
        var curV = Mathf.Clamp(cur / max, 0, 1);
        var targetV = Mathf.Clamp(target / max, 0, 1);

        if (cur > target)
        {
            DrawBarRect(center, height, width, curV, style[TeamType].ShieldCurrentColor);
            DrawFlashingRect(center, height, width, targetV, curV, GetLighterColor(style[TeamType].ShieldCurrentColor, -ColorLightChange), flash);
        }
        else if (cur < target)
        {
            DrawBarRect(center, height, width, curV, style[TeamType].ShieldCurrentColor);
            DrawFlashingRect(center, height, width, curV, targetV, GetLighterColor(style[TeamType].ShieldCurrentColor, ColorLightChange), flash);
        }
        else
        {
            DrawBarRect(center, height, width, curV, style[TeamType].ShieldCurrentColor);
        }
    }

    private void DrawFlashingRect(Vector2 center, float height, float width, float fromV, float toV, in Color color, float flash)
    {
        var minV = Mathf.Min(fromV, toV);
        var maxV = Mathf.Max(fromV, toV);
        var startX = center.X - width / 2 + minV * width;
        var rectWidth = (maxV - minV) * width;

        var baseColor = color;
        var flashColor = new Color(baseColor.R, baseColor.G, baseColor.B, baseColor.A * flash);
        DrawRect(new Rect2(startX, center.Y - height / 2, rectWidth, height), flashColor);
    }

    private void DrawBarRect(Vector2 center, float height, float width, float v, in Color color)
    {
        var vwidth = width * Mathf.Clamp(v, 0, 1);
        DrawRect(new Rect2(center.X - width / 2, center.Y - height / 2, vwidth, height), color);
    }

    private void DrawSplitLine(Vector2 center, float height, float totalWidth, int blockCount, GameViewStyle style)
    {
        for (int i = 1; i < blockCount; i++)
        {
            float x = center.X - totalWidth / 2 + i * BlockWidth;
            DrawLine(
                new Vector2(x, center.Y - height / 2),
                new Vector2(x, center.Y + height / 2),
                style[TeamType].SplitColor,
                1
            );
        }
    }
}