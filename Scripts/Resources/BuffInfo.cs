using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool]
public partial class BuffInfo : Resource, IInfo
{
    public static readonly BuffInfo Null = new BuffInfo()
    {
        Name = "None",
        Description = "空占位Buff"
    };
    [Export] public string Name { get; set; } = "None";
    [Export] public Texture2D? Icon { get; set; }
    [Export] public int TurnRemoveCount { get; set; }
    /// <summary>
    /// 优先级，越低越先执行
    /// </summary>
    [Export] public int Priority { get; set; } = 0;
    [Export] public Vector2 ViewOffset { get; set; } = Vector2.Zero;
    [Export] public SpriteFrames? ViewFrames { get; set; }
    [Export] public string Description { get; set; } = string.Empty;
    public IBuff Hook { get => field ??= new EmptyBuff(Name); set => field ??= value; }
}

public enum BuffMode
{
    Single,
    Counter,
}