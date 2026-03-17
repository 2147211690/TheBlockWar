using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class UnitState : IReadOnlyUnitState
{
    public static readonly IReadOnlyUnitState Default = new UnitState();
    public uint Id { get; set; }
    public int Team { get; set; } = 0;
    public string Name { get; set; } =  string.Empty;
    public Vector2I Coords { get; set; }
    public float Health { get; set; } = 100;
    public float MaxHealth { get; set; } = 100;
    public float Shield { get; set; } = 100;
    public float MaxShield { get; set; } = 100;
    public bool IsDead { get; set; }
    public bool IsFlying { get; set; } = false;
    public IReadOnlyList<BuffState> Buffs => _buffs ?? [];
    private List<BuffState>? _buffs;
    
    /// <summary>
    /// 添加一个buff,不做检查
    /// </summary>
    public void AddBuff(BuffTypeEnum buffType, int duration)
    {
        GetBuffList().Add(new BuffState
        {
            BuffType = buffType,
            Duration = duration
        });
    }

    public void RemoveBuff(BuffTypeEnum buffType)
    {
        if (!this.TryFindBuff(buffType, out var index)) return;
        GetBuffList().RemoveAt(index);
    }
    public void RemoveBuff(int index)
    {
        if (index < 0 || index >= Buffs.Count) return;
        GetBuffList().RemoveAt(index);
    }

    public void ModifyBuff(BuffTypeEnum buffType, int modifyDuration)
    {
        if (!this.TryFindBuff(buffType, out var index)) return;
        GetBuffList()[index] = new BuffState
        {
            BuffType = buffType,
            Duration =  modifyDuration
        };
    }
    public void ModifyBuff(int index, int modifyDuration)
    {
        if (index < 0 || index >= Buffs.Count) return;
        GetBuffList()[index] = new BuffState
        {
            BuffType = Buffs[index].BuffType,
            Duration =  modifyDuration
        };
    }
    
    private List<BuffState> GetBuffList() =>  _buffs ??= [];
    public UnitState Clone()
    {
        return CopyTo(new UnitState());
    }
    public UnitState CopyTo(UnitState unit)
    {
        unit.Id = Id;
        unit.Team = Team;
        unit.Name = Name;
        unit.Coords = Coords;
        unit.Health = Health;
        unit.MaxHealth = MaxHealth;
        unit.Shield = Shield;
        unit.MaxShield = MaxShield;
        unit.IsDead = IsDead;
        unit.IsFlying = IsFlying;
        unit._buffs = _buffs?.Count > 0 ? _buffs.CloneToList() : null;
        return unit;
    }
}