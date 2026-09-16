namespace TheBlockWar.Scripts.Games.GameStates;

/// <summary>
/// 记录单位在下回合准备使用的技能。
/// </summary>
public readonly record struct PrepareSkillInfo(string SkillName, Vector2I?[]? Args);