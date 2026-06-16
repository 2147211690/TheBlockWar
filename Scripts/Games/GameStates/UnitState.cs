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
            Buffs = _buffs.CloneToListOrNullWhenCountZero(),
        };
    }   
    /// <summary>
    /// 添加一个buff,不做检查
    /// </summary>
    public void AddBuff(string buffName, int count)
    {
        var list = GetBuffList();
        for (var i = 0; i < list.Count; i++)
        {
            if (list[i].BuffName.GetBuffInfo().Priority < buffName.GetBuffInfo().Priority) continue;
            list.Insert(i, new BuffState
            {
                BuffName = buffName,
                Count = count
            });
            _isBuffStaticDirty = true;
            return;
        }
        list.Add(new BuffState
        {
            BuffName = buffName,
            Count = count
        });
        _isBuffStaticDirty = true;
    }

    public void RemoveBuff(string buffName)
    {
        if (!this.TryFindBuff(buffName, out var index)) return;
        var list = GetBuffList();
        list.RemoveAt(index);
        _isBuffStaticDirty = true;
    }
    public void RemoveBuff(int index)
    {
        if (index < 0 || index >= Buffs.Count) return;
        var list = GetBuffList();
        list.RemoveAt(index);
        _isBuffStaticDirty = true;
    }

    public void ClearBuff()
    {
        GetBuffList().Clear();
        _isBuffStaticDirty = true;
    }

    public void ModifyBuff(string buffName, int modifyDuration)
    {
        if (!this.TryFindBuff(buffName, out var index)) return;
        var list = GetBuffList();
        list[index] = new BuffState
        {
            BuffName = buffName,
            Count =  modifyDuration
        };
        _isBuffStaticDirty = true;
    }
    public void ModifyBuff(int index, int modifyCount)
    {
        if (index < 0 || index >= Buffs.Count) return;
        var list = GetBuffList();
        list[index] = new BuffState
        {
            BuffName = Buffs[index].BuffName,
            Count =  modifyCount
        };
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
        unit._buffsStatic = _isBuffStaticDirty ? null : _buffsStatic;
        return unit;
    }


}