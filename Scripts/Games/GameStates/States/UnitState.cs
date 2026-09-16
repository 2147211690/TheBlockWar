using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;
public class UnitState : IReadOnlyUnitState
{
    public static readonly UnitState Null = IReadOnlyUnitState.Null.Clone();
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
    public CanUseSkillMask CanUseSkill { get; set; } = CanUseSkillMask.All;
    public PrepareSkillInfo? PrepareSkill { get; set; }
    public IReadOnlyList<BuffState> Buffs => _buffs ?? [];
    public IReadOnlyList<BuffState> BuffsStatic
    {
        get
        {
            if (_isBuffStaticDirty)
            {
                _buffsStatic = _buffs.CloneToListOrNullWhenCountZero();
                _isBuffStaticDirty = false;
            }
            return _buffsStatic ?? [];
        }
    }
    [JsonInclude]
    private List<BuffState>? _buffs;
    private List<BuffState>? _buffsStatic;
    private bool _isBuffStaticDirty = true;
    public UnitState(){}

    public UnitState(UnitStateData data)
    {
        Id = data.Id;
        Team = data.Team;
        Name = data.Name;
        Coords = data.Coords;
        Health = data.Health;
        MaxHealth = data.MaxHealth;
        Shield = data.Shield;
        MaxShield = data.MaxShield;
        IsDead = data.IsDead;
        IsFlying = data.IsFlying;
        CanUseSkill = data.CanUseSkillMask;
        _buffs = data.Buffs.CloneToListOrNullWhenCountZero();
    }
    public UnitStateData ToData()
    {
        return new UnitStateData
        {
            Id = Id,
            Team = Team,
            Name = Name,
            Coords = Coords,
            Health = Health,
            MaxHealth = MaxHealth,
            Shield = Shield,
            MaxShield = MaxShield,
            IsDead = IsDead,
            IsFlying = IsFlying,
            CanUseSkillMask = CanUseSkill,
            Buffs = _buffs.CloneToListOrNullWhenCountZero(),
        };
    }

    public UnitState GetSnapshot()
    {
        return Clone();
    }

    public int FindBuff(string buffName)
    {
        return _buffs?.FindIndex(a => a.BuffName == buffName) ?? -1;
    }
    
    public int AddBuff(string buffName, int count)
    {
        if (count < 0) count = int.MaxValue;
        var list = GetBuffList();
        var index = FindBuff(buffName);
        var span = CollectionsMarshal.AsSpan(_buffs);
        _isBuffStaticDirty = true;
        if (index >= 0)
        {
            span[index].Count = count == int.MaxValue ? int.MaxValue : span[index].Count + count;
            return index;
        }
        else
        {
            var p = buffName.GetBuffInfo().Priority;
            var i = 0;
            for (; i < span.Length; i++)
                if (span[i].BuffName.GetBuffInfo().Priority > p) break;
            list.Insert(i, new BuffState()
            {
                BuffName = buffName,
                Count = count
            });
            return i;
        }
    }

    public int RemoveBuff(string buffName, int count)
    {
        var index = FindBuff(buffName);
        if (index < 0) return -1;
        return RemoveBuff(index, count);
    }
    public int RemoveBuff(int index, int count)
    {
        if (index < 0 || index >= Buffs.Count) return -1;
        var list = GetBuffList();
        var span = CollectionsMarshal.AsSpan(_buffs);
        span[index].Count = Mathf.Max(0, span[index].Count - (count < 0 ? int.MaxValue : count));
        _isBuffStaticDirty = true;
        if (span[index].Count == 0)
        {
            list.RemoveAt(index);
            return -1;
        }
        else
        {
            return index;
        }
    }

    public void ClearBuff()
    {
        GetBuffList().Clear();
        _isBuffStaticDirty = true;
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
        unit._buffs = _buffs.CloneToListOrNullWhenCountZero();
        unit._isBuffStaticDirty = _isBuffStaticDirty;
        unit.CanUseSkill = CanUseSkill;
        unit.PrepareSkill = PrepareSkill;
        unit._buffsStatic = _isBuffStaticDirty ? null : _buffsStatic;
        return unit;
    }


}