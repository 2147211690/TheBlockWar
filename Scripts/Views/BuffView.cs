using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views;

[GlobalClass]
public partial class BuffView : Node2D
{
    [Export] public GameViewStyle Style { get; set; } = null!;
    [Export] public TeamTypeEnum TeamType { get; set; }
    [Export] public SpriteFrames BuffEffectSpriteFrames { get; set; } = null!;
    [Export] public Godot.Collections.Dictionary<string, Vector2> BuffEffectOffsets { get; set; } = null!;
    [Export] public UnitView UnitView { get; set; } = null!;
    // 【元素外框尺寸】默认值匹配截图
    [Export] public Vector2 SymbolBoxSize { get; private set; } = new(24, 24);
    [Export] public Vector2 IconBoxSize { get; private set; } = new(32, 32);

    // 【边距间隔】默认值匹配截图
    [Export] public float ElementSpacing { get; private set; } = 2f;
    [Export] public float RowSpacing { get; private set; } = 4f;

    // 加减号纹理
    [ExportGroup("符号纹理")]
    [Export] public Texture2D PlusTexture { get; set; }
    [Export] public Texture2D MinusTexture { get; set; }

    // 【动画效果】默认值匹配截图
    [ExportGroup("动画效果")]
    [Export] public float AnimationSpeed { get; set; } = 8f;
    [Export] public float GainHighlightMin { get; set; } = 0.6f;
    [Export] public float GainHighlightMax { get; set; } = 1f;
    [Export] public float LoseDimMin { get; set; } = 0.2f;
    [Export] public float LoseDimMax { get; set; } = 0.6f;

    // 【新增：颜色调制】给buff符号+图标统一上色
    [ExportGroup("颜色调制")]
    [Export] public Color GainTint { get; set; } = Colors.LimeGreen;   // 增益buff颜色（加号+图标）
    [Export] public Color LoseTint { get; set; } = Colors.OrangeRed;    // 减益buff颜色（减号+图标）

    
    public GameView GameView { get; set; } = null!;
    // ===================== 缓存列表（无GC） =====================
    private readonly List<BuffTypeEnum> _gainBuffs = new();
    private readonly List<BuffTypeEnum> _loseBuffs = new();
    private double _animationTime;
    private float _gainAlpha;
    private float _loseAlpha;
    
    // Buff数据
    private readonly Dictionary<BuffTypeEnum, int> _buffChanges = new();
    private readonly Dictionary<BuffTypeEnum, AnimatedSprite2D> _visibleBuffs = new();
    private bool _refreshBuffListsDirty = true;

    public void Init(GameView gameView, GameViewStyle style, TeamTypeEnum teamType)
    {
        GameView = gameView;
        Style = style;
        TeamType = teamType;
    }

    public override void _Process(double delta)
    {
        if (_gainBuffs.Count == 0 && _loseBuffs.Count == 0) return;

        _animationTime += delta * AnimationSpeed;
        float wave = (Mathf.Sin((float)_animationTime) + 1) / 2;

        // 周期透明度计算
        _gainAlpha = Mathf.Lerp(GainHighlightMin, GainHighlightMax, wave);
        _loseAlpha = Mathf.Lerp(LoseDimMin, LoseDimMax, wave);

        QueueRedraw();
    }

    public void ChangeBuffChange(BuffTypeEnum type, int change)
    {
        if (!_buffChanges.TryAdd(type, change))
            _buffChanges[type] += change;

        if (_buffChanges[type] == 0)
            _buffChanges.Remove(type);

        _refreshBuffListsDirty = true;
        QueueRedraw();
    }
    public void AddVisibleBuff(BuffTypeEnum type)
    {
        if (_visibleBuffs.ContainsKey(type)) return;
        var s = GameView.ViewPools.AnimatedSprite2DPool.Pop();
        s.GetParent().RemoveChild(s);
        UnitView.Center.AddChild(s);
        s.Position = Vector2.Zero;
        _visibleBuffs.Add(type, s);
        if (BuffEffectOffsets.TryGetValue(Enum.GetName(type)!, out var offset)) s.Offset = offset;
        
        s.SpriteFrames = BuffEffectSpriteFrames;
        var name = $"{Enum.GetName(type)}_Add";
        var idleName = $"{Enum.GetName(type)}_Idle";
        if (BuffEffectSpriteFrames.HasAnimation(name))
        {
            s.Play(name);
            if (BuffEffectSpriteFrames.HasAnimation(idleName))
            {
                s.ContinueFinishedOneShot(() => s.Play(idleName));
            }
        }
        else if (BuffEffectSpriteFrames.HasAnimation(idleName)) s.Play(idleName);
        
    }
    public void RemoveVisibleBuff(BuffTypeEnum type)
    {
        if (!_visibleBuffs.Remove(type, out var s)) return;

        var name = $"{Enum.GetName(type)}_Remove";
        if (BuffEffectSpriteFrames.HasAnimation(name))
        {
            s.Play(name);
            s.ContinueFinishedOneShot(() => AfterAnim(s));
        }
        else AfterAnim(s);

        void AfterAnim(AnimatedSprite2D animatedSprite2D)
        {
            animatedSprite2D.GetParent().RemoveChild(animatedSprite2D);
            GameView.DrawContainer.AddChild(animatedSprite2D);
            animatedSprite2D.SpriteFrames = null;
            animatedSprite2D.Offset = Vector2.Zero;
            GameView.ViewPools.AnimatedSprite2DPool.Push(animatedSprite2D);
        }
    }
    public override void _Draw()
    {
        base._Draw();
        if (_refreshBuffListsDirty)
        {
            RefreshBuffLists();
            _refreshBuffListsDirty = false;
        }
        if (_gainBuffs.Count == 0 && _loseBuffs.Count == 0) return;

        int rowCount = (_gainBuffs.Count > 0 ? 1 : 0) + (_loseBuffs.Count > 0 ? 1 : 0);
        float singleRowHeight = Mathf.Max(SymbolBoxSize.Y, IconBoxSize.Y);
        float totalHeight = rowCount * singleRowHeight + (rowCount > 1 ? RowSpacing : 0);
        float currentRowCenterY = -totalHeight / 2 + singleRowHeight / 2;

        if (_gainBuffs.Count > 0)
        {
            DrawRow(new Vector2(0, currentRowCenterY), _gainBuffs, true, _gainAlpha);
            currentRowCenterY += singleRowHeight + RowSpacing;
        }
        if (_loseBuffs.Count > 0)
            DrawRow(new Vector2(0, currentRowCenterY), _loseBuffs, false, _loseAlpha);
    }

    /// <summary>
    /// 仅数据变化时刷新缓存列表
    /// </summary>
    private void RefreshBuffLists()
    {
        _gainBuffs.Clear();
        _loseBuffs.Clear();

        foreach (var kvp in _buffChanges)
        {
            if (kvp.Value > 0) _gainBuffs.Add(kvp.Key);
            else if (kvp.Value < 0 && _visibleBuffs.ContainsKey(kvp.Key)) _loseBuffs.Add(kvp.Key);
        }
    }

    /// <summary>
    /// 绘制单行（带颜色+透明度）
    /// </summary>
    private void DrawRow(Vector2 rowCenter, List<BuffTypeEnum> buffs, bool isGain, float alpha)
    {
        float totalWidth = SymbolBoxSize.X;
        if (buffs.Count > 0)
        {
            totalWidth += ElementSpacing + buffs.Count * IconBoxSize.X;
            totalWidth += (buffs.Count - 1) * ElementSpacing;
        }

        float currentX = rowCenter.X - totalWidth / 2;
        float rowCenterY = rowCenter.Y;
        Color currentTint = isGain ? GainTint : LoseTint; // 选择对应颜色
        Texture2D symbolTex = isGain ? PlusTexture : MinusTexture;

        // 绘制符号
        if (symbolTex != null)
        {
            DrawTextureInBox(
                symbolTex,
                new Vector2(currentX, rowCenterY - SymbolBoxSize.Y / 2),
                SymbolBoxSize,
                currentTint,
                alpha
            );
        }
        currentX += SymbolBoxSize.X + ElementSpacing;

        // 绘制图标
        foreach (var type in buffs)
        {
            var icon = type.GetInstance().Info.Icon;
            if (icon != null)
            {
                DrawTextureInBox(
                    icon,
                    new Vector2(currentX, rowCenterY - IconBoxSize.Y / 2),
                    IconBoxSize,
                    currentTint,
                    alpha
                );
            }
            currentX += IconBoxSize.X + ElementSpacing;
        }
    }

    /// <summary>
    /// 统一绘制：等比缩放+居中+不超出+颜色调制+透明度
    /// </summary>
    private void DrawTextureInBox(Texture2D tex, Vector2 boxTopLeft, Vector2 boxSize, Color tint, float alpha)
    {
        float scale = Mathf.Min(boxSize.X / tex.GetWidth(), boxSize.Y / tex.GetHeight());
        Vector2 fitSize = new(tex.GetWidth() * scale, tex.GetHeight() * scale);
        Vector2 offset = (boxSize - fitSize) / 2;
        Vector2 drawPos = boxTopLeft + offset;

        // 合并颜色与透明度（颜色保留，透明度由动画控制）
        Color finalColor = new Color(tint.R, tint.G, tint.B, alpha);
        DrawTextureRect(tex, new Rect2(drawPos, fitSize), false, finalColor);
    }

    // ===================== 原有接口完全保留 =====================
    public void ClearAllBuffChanges()
    {
        _buffChanges.Clear();
        RefreshBuffLists();
        QueueRedraw();
    }

    public void SetBuffChange(BuffTypeEnum type, int value)
    {
        _buffChanges[type] = value;
        if (value == 0) _buffChanges.Remove(type);
        RefreshBuffLists();
        QueueRedraw();
    }

    public int GetBuffChange(BuffTypeEnum type) => _buffChanges.GetValueOrDefault(type, 0);
}