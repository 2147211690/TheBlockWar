namespace TheBlockWar.Scripts.Games.Turns;

/// <summary>
/// 阶段顺序查找表:用于记录并读取各类下回合触发者的顺序数字。
/// <para>Buff / Region 按类别存一个顺序数字;Skill / PlaceUnit 需按队伍分别存。</para>
/// <para>未设置时读取返回 <see cref="int.MaxValue"/>(视为排到末位)。</para>
/// </summary>
public class PhaseOrderTable
{
    private readonly Dictionary<PhaseEnum, int> _phaseOrder = new();
    private readonly Dictionary<(PhaseEnum Kind, int Team), int> _teamOrder = new();

    public int GetOrder(PhaseEnum kind)
        => _phaseOrder.TryGetValue(kind, out var v) ? v : int.MaxValue;
    public void SetOrder(PhaseEnum kind, int order) => _phaseOrder[kind] = order;
    public int GetOrder(PhaseEnum kind, int team)
        => _teamOrder.TryGetValue((kind, team), out var v) ? v : int.MaxValue;
    public void SetOrder(PhaseEnum kind, int team, int order) => _teamOrder[(kind, team)] = order;

    public int GetBuffOrder() => GetOrder(PhaseEnum.Buff);
    public void SetBuffOrder(int order) => SetOrder(PhaseEnum.Buff, order);

    public int GetRegionOrder() => GetOrder(PhaseEnum.Region);
    public void SetRegionOrder(int order) => SetOrder(PhaseEnum.Region, order);

    public int GetSkillOrder(int team) => GetOrder(PhaseEnum.Skill, team);
    public void SetSkillOrder(int team, int order) => SetOrder(PhaseEnum.Skill, team, order);

    public int GetPlaceUnitOrder(int team) => GetOrder(PhaseEnum.PlaceUnit, team);
    public void SetPlaceUnitOrder(int team, int order) => SetOrder(PhaseEnum.PlaceUnit, team, order);
}