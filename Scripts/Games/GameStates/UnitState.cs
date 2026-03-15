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
    /// 添加一个buff,如果buff已存在,则修改其持续时间为最大持续时间
    /// </summary>
    /// <param name="buff"></param>
    public void AddBuff(BuffTypeEnum buffType, int duration)
    {
        if (this.TryFindBuff(buffType, out var index))
        {
            var oldBuffDuration = GetBuffList()[index].Duration;
            GetBuffList()[index] = new BuffState
            {
                BuffType = buffType,
                Duration = Mathf.Max(oldBuffDuration, duration)
            };
        }
        else
        {
            GetBuffList().Add(new BuffState
            {
                BuffType = buffType,
                Duration = duration
            });
        }
    }

    public void RemoveBuff(BuffTypeEnum buffType)
    {
        if (!this.TryFindBuff(buffType, out var index)) return;
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
        unit._buffs = _buffs?.CloneToList();
        return unit;
    }
}