using TheBlockWar.Scripts.Games.Units;

namespace TheBlockWar.Scripts.Games.GameStates.Datas;

public class UnitStateData
{
    public long Id { get; set; }
    public int Team { get; set; } = 0;
    public UnitNameEnum Name { get; set; }
    public Vector2I Coords { get; set; }
    public float Health { get; set; } = 100;
    public float MaxHealth { get; set; } = 100;
    public float Shield { get; set; } = 100;
    public float MaxShield { get; set; } = 100;
    public bool IsDead { get; set; }
    public bool IsFlying { get; set; } = false;
    public List<BuffState>? Buffs { get; set; }
}