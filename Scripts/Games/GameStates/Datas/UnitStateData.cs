namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class UnitStateData
{
    public long Id { get; set; }
    public int Team { get; set; } = 0;
    public string Name { get; set; } = string.Empty;
    public Vector2I Coords { get; set; }
    public float Health { get; set; } = 100;
    public float MaxHealth { get; set; } = 100;
    public float Shield { get; set; } = 100;
    public float MaxShield { get; set; } = 100;
    public bool IsDead { get; set; }
    public bool IsFlying { get; set; } = false;
    public SkillPrepareInfo? PreparedSkill { get; set; }
    public CanUseSkillMask CanUseSkillMask { get; set; } = CanUseSkillMask.All;
    public List<BuffState>? Buffs { get; set; }
}