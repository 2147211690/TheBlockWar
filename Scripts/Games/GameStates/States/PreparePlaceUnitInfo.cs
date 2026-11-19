namespace TheBlockWar.Scripts.Games.GameStates;

/// <summary>
/// 队伍待放置单位信息:坐标 + 单位名,代表该队伍本回合计划放置在某个格子的单位。
/// 槽位序号不作为数据的一部分,由 SetTeamPreparePlaceUnit 的 index 参数定位。
/// </summary>
public readonly record struct PreparePlaceUnitInfo(Vector2I Coords, string UnitName);