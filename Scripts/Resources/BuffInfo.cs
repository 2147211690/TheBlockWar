using Godot;
using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass]
public partial class BuffInfo : Resource
{
    public static readonly BuffInfo Null = new BuffInfo()
    {
        BuffType = BuffTypeEnum.None,
        Description = "空占位Buff"
    };
    [Export] public BuffTypeEnum BuffType { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    /// <summary>
    /// 优先级，越低越先执行
    /// </summary>
    [Export] public int Priority { get; set; } = 0;
    [Export] public string Description { get; set; } = string.Empty;
}