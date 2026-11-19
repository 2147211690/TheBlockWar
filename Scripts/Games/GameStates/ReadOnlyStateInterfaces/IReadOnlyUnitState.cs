using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitState : IReadOnlyGameDataState<UnitState, UnitStateData>
{
    public static readonly IReadOnlyUnitState Null = new UnitState()
    {
        Id = 0,
        Coords = new Vector2I(int.MinValue, int.MinValue),
        IsDead = true,
    };
    long Id { get; }
    int Team { get; }
    string Name { get; }
    Vector2I Coords { get; }
    float Health { get; }
    float MaxHealth { get; }
    float Shield { get; }
    float MaxShield { get; }
    bool IsDead { get; }
    bool IsFlying { get; }
    SkillPrepareInfo? PreparedSkill { get; }
    CanUseSkillMask CanUseSkill { get; }
    /// <summary>
    /// 单位在下回合准备使用的技能(为空表示本回合无技能使用)
    /// </summary>
    PrepareSkillInfo? PrepareSkill { get; }
    IReadOnlyList<BuffState> Buffs { get; }
    /// <summary>
    /// 获取buff的静态列表,更新buff时不会改变列表中的元素
    /// </summary>
    IReadOnlyList<BuffState> BuffsStatic { get; }
    int FindBuff(string buffName);
    UnitState Clone();
    UnitState CopyTo(UnitState unit);
}
[Flags]
public enum CanUseSkillMask
{
    None = 0,
    Move = 1 << 0,
    Skill = 1 << 1,
    All = Move | Skill,
}
