using TheBlockWar.Scripts.Games.Buffs;

namespace TheBlockWar.Scripts.Resources;
[GlobalClass, Tool]
public partial class BuffInfo : Resource, IInfo<BuffNameEnum>
{
    public static readonly BuffInfo Null = new BuffInfo()
    {
        Name = BuffNameEnum.None,
        Description = "空占位Buff"
    };
    [Export] public BuffNameEnum Name { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    /// <summary>
    /// 优先级，越低越先执行
    /// </summary>
    [Export] public int Priority { get; set; } = 0;
    [Export] public string Description { get; set; } = string.Empty;

}