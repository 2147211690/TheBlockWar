using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using Autoload = TheBlockWar.Scripts.Tools.Autoload;

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
    [Export] public Vector2 IconBoxSize { get; private set; } = new(32, 32);

    // 【边距间隔】默认值匹配截图
    [Export] public float ElementSpacing { get; private set; } = 2f;
 
    // 加减号纹理
    [ExportGroup("符号纹理")]
    [Export] public Texture2D? PlusTexture { get; set; }
    [Export] public Texture2D? MinusTexture { get; set; }

    // 【动画效果】默认值匹配截图
    [ExportGroup("动画效果")]
    [Export] public float AnimationSpeed { get; set; } = 8f;
    [Export] public float GainHighlightMin { get; set; } = 0.6f;
    [Export] public float GainHighlightMax { get; set; } = 1f;
    [Export] public float LoseDimMin { get; set; } = 0.2f;
    [Export] public float LoseDimMax { get; set; } = 0.6f;

    
    public GameView GameView { get; set; } = null!;
    // ===================== 缓存列表（无GC） =====================
    private readonly List<BuffTypeEnum> _gainBuffs = new();
    private readonly List<BuffTypeEnum> _loseBuffs = new();
    private double _animationTime;
    private float _gainLight;
    private float _loseLight;
    
    // Buff数据
    private readonly Dictionary<BuffTypeEnum, int> _buffChanges = new();
    private readonly Dictionary<BuffTypeEnum, AnimatedSprite2D> _visibleBuffs = new();
    private bool _refreshBuffListsDirty = true;
    private ItemTabStyle _gainItemTabStyle = new()
    {
        ItemSize = new(12,12),
        ItemSpacing = 0,
        Layout = -1,
        Modulate = Colors.White
    };

    private ItemTabStyle _loseTabStyle = new()
    {
        ItemSize = new(12, 12),
        ItemSpacing = 0,
        Layout = -1,
        Modulate = Colors.White
    };
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
        _gainLight = Mathf.Lerp(GainHighlightMin, GainHighlightMax, wave);
        _loseLight = Mathf.Lerp(LoseDimMin, LoseDimMax, wave);

        QueueRedraw();
    }

    public void ChangeBuffChange(BuffTypeEnum type, int change)
    {
        if (change == 0) return;
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
        var addName = $"{Enum.GetName(type)}_Add";
        var idleName = $"{Enum.GetName(type)}_Idle";
        if (BuffEffectSpriteFrames.HasAnimation(addName))
        {
            s.Play(addName);
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

        // 设置样式
        _gainItemTabStyle.ItemSize = _loseTabStyle.ItemSize = IconBoxSize;
        _gainItemTabStyle.Modulate = _gainItemTabStyle.Modulate with { V = _gainLight };
        _loseTabStyle.Modulate = _loseTabStyle.Modulate with { V = _loseLight };
        _gainItemTabStyle.ItemSpacing = _loseTabStyle.ItemSpacing = ElementSpacing;

        // 构建图标列表
        var gainIcons = BuildIconList(_gainBuffs, PlusTexture);
        var loseIcons = BuildIconList(_loseBuffs, MinusTexture);

        // 计算长度
        var gainLength = CalculateLength(gainIcons);
        var loseLength = CalculateLength(loseIcons);

        // 计算位置
        var p = (gainLength + loseLength) / 2;
        var offset = gainIcons != null && loseIcons != null ? new Vector2(ElementSpacing / 2, 0) : Vector2.Zero;
        var losePosition = new Vector2(-(loseLength - p), 0) + offset;
        var gainPosition = losePosition + new Vector2(-gainLength, 0) - offset;

        // 绘制
        if (gainIcons != null) this.DrawItemTab(gainPosition, gainIcons, _gainItemTabStyle);
        if (loseIcons != null) this.DrawItemTab(losePosition, loseIcons, _loseTabStyle);
        
        List<Texture2D> BuildIconList(List<BuffTypeEnum> buffs, Texture2D defaultTexture)
        {
            if (buffs.Count == 0) return null;

            var icons = new List<Texture2D>(buffs.Count + 1);
            icons.Add(defaultTexture ?? Autoload.DefaultTexture);

            foreach (var buff in buffs)
            {
                icons.Add(buff.GetInstance().Info.Icon ?? Autoload.DefaultTexture);
            }

            return icons;
        }
        float CalculateLength(List<Texture2D>? icons)
        {
            if (icons == null || icons.Count == 0) return 0;
            return icons.Count * IconBoxSize.X + ElementSpacing * (icons.Count - 1);
        }
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
    
    public void ClearAllBuffChanges()
    {
        _buffChanges.Clear();
        RefreshBuffLists();
        QueueRedraw();
    }
}