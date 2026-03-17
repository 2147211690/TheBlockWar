using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class UnitMapState : IUnitMapState
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    private Dictionary<uint, UnitState> _units = new();
    private Dictionary<Vector2I, uint> _unitAt = new();

    public UnitMapState(Vector2I size)
    {
        Size = size;
    }

    public IReadOnlyUnitMapState? Owner => null;
    public Vector2I Size { get; }
    public IEnumerable<IReadOnlyUnitState> GetAllUnits() => _units.Values;

    public bool HasUnit(uint id)
    {
        return _units.ContainsKey(id);
    }

    public bool HasUnit(Vector2I coords)
    {
        return _unitAt.ContainsKey(coords);
    }

    public IReadOnlyUnitState GetUnit(uint id) => _units.GetValueOrDefault(id) ?? IReadOnlyUnitState.Null;

    public IReadOnlyUnitState GetUnit(Vector2I coords) =>
        _unitAt.TryGetValue(coords, out var id)
            ? (_units.GetValueOrDefault(id) ?? IReadOnlyUnitState.Null)
            : IReadOnlyUnitState.Null;

    public IUnitMapState GetSnapshot()
    {
        return new UnitMapStateSnapshot(this);
    }

    public void AddUnit(UnitState unit)
    {
        if (_units.TryGetValue(unit.Id, out var old))
        {
            Logger.Warn("单位Id{id}已经存在于数据上,无法添加", unit.Id);
            return;
        }

        if (!unit.Coords.IsInSize(Size))
        {
            Logger.Warn("单位{id}坐标{coords}超出地图范围", unit.Id, unit.Coords);
            return;
        }

        _units[unit.Id] = unit;
        _unitAt[unit.Coords] = unit.Id;
    }

    public void RemoveUnit(uint id)
    {
        if (!_units.Remove(id, out var unit)) return;
        _unitAt.Remove(unit.Coords);
    }

    public void ModifyUnit(uint id, Action<UnitState> modifier)
    {
        if (!_units.TryGetValue(id, out var unitState))
        {
            Logger.Warn("单位Id{id}不存在于数据上", id);
            return;
        }

        var oldCoords = unitState.Coords;

        modifier(unitState);
        unitState.Id = id; //确保ID一致

        if (oldCoords != unitState.Coords)
        {
            _unitAt.Remove(oldCoords);
            _unitAt[unitState.Coords] = id;
        }
    }

    public void ClearIfSnapshot()
    {
    }

    /// <summary>
    /// 单位地图状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// 读取保证：首次读取自动克隆到本地修改表，永久返回克隆体，绝不返回Owner原始引用
    /// </summary>
    public sealed class UnitMapStateSnapshot : IUnitMapState
    {
        // 父状态引用 - 未修改数据的回退源
        public IReadOnlyUnitMapState Owner { get; }
        public Vector2I Size => Owner.Size;

        // 增量存储：仅包含相对于owner的变更
        private Dictionary<uint, UnitState>? _modifiedUnits; // 修改/新增的单位 + 读时克隆的单位
        private HashSet<uint>? _removedUnitIds; // 删除的单位ID
        private Dictionary<Vector2I, uint>? _modifiedUnitAt; // 坐标变更（仅包含变更的坐标）

        private List<UnitState>? _pooledUnits;

        //池
        private static readonly TypedPool<UnitState> UnitPool = new(() => new UnitState(), 10);

        // 脏标记：用于延迟初始化
        private bool _isModified;

        public UnitMapStateSnapshot(IReadOnlyUnitMapState owner)
        {
            // 只能是UnitMapState或其快照
            Owner = owner;
        }

        private Dictionary<uint, UnitState> GetModifiedUnits() =>
            _modifiedUnits ??= new Dictionary<uint, UnitState>();

        private HashSet<uint> GetRemovedIds() =>
            _removedUnitIds ??= new HashSet<uint>();

        private Dictionary<Vector2I, uint> GetModifiedCoords() =>
            _modifiedUnitAt ??= new Dictionary<Vector2I, uint>();

        private List<UnitState> GetPooledUnits() =>
            _pooledUnits ??= new List<UnitState>();

        public bool HasUnit(uint id)
        {
            if (_removedUnitIds?.Contains(id) == true)
                return false;
            if (_modifiedUnits?.ContainsKey(id) == true)
                return true;
            return Owner.HasUnit(id);
        }

        public bool HasUnit(Vector2I coords)
        {
            // 检查坐标是否被本地修改（包括删除导致的空位）
            if (_modifiedUnitAt?.TryGetValue(coords, out var localId) == true)
            {
                // 如果是0表示该坐标被清空
                return localId != 0;
            }

            // 检查该坐标的单位是否被删除
            var ownerId = Owner.GetUnit(coords).Id;
            if (ownerId != 0 && _removedUnitIds?.Contains(ownerId) == true)
                return false;

            return Owner.HasUnit(coords);
        }

        /// <summary>
        /// 核心：首次读取自动克隆到本地修改表，永久返回克隆体
        /// </summary>
        public IReadOnlyUnitState GetUnit(uint id)
        {
            // 1. 已删除 → 返回空
            if (_removedUnitIds?.Contains(id) == true)
                return IReadOnlyUnitState.Null;

            var modifiedUnits = GetModifiedUnits();

            // 2. 本地已有 → 直接返回（永久缓存）
            if (modifiedUnits.TryGetValue(id, out var unit))
                return unit;

            // 3. 首次读取：从Owner克隆 → 存入本地 → 永久使用
            var ownerUnit = Owner.GetUnit(id);
            if (ownerUnit.IsNull())
                return IReadOnlyUnitState.Null;

            // 克隆并加入本地修改表（读时COW）
            var clone = ownerUnit.CopyTo(UnitPool.Pop());
            GetPooledUnits().Add(clone);
            modifiedUnits[id] = clone;
            _isModified = true; // 读取也算作状态变更（因为产生了本地副本）

            return clone;
        }

        /// <summary>
        /// 坐标获取：同样保证返回本地克隆体
        /// </summary>
        public IReadOnlyUnitState GetUnit(Vector2I coords)
        {
            // 1. 本地坐标修改
            if (_modifiedUnitAt?.TryGetValue(coords, out var localId) == true)
            {
                if (localId == 0) return IReadOnlyUnitState.Null;
                return GetUnit(localId); // 走ID获取，自动保证克隆
            }

            // 2. 父状态坐标 → 先拿ID，再走ID获取（自动克隆）
            var ownerUnit = Owner.GetUnit(coords);
            if (ownerUnit.IsNull())
                return IReadOnlyUnitState.Null;

            if (_removedUnitIds?.Contains(ownerUnit.Id) == true)
                return IReadOnlyUnitState.Null;

            return GetUnit(ownerUnit.Id); // 关键：全部走ID获取，确保本地克隆
        }

        /// <summary>
        /// 遍历：全部返回本地克隆体
        /// </summary>
        public IEnumerable<IReadOnlyUnitState> GetAllUnits()
        {
            var processed = new HashSet<uint>();

            // 1. 遍历本地已修改/已克隆的单位
            if (_modifiedUnits != null)
            {
                foreach (var (id, unit) in _modifiedUnits)
                {
                    if (_removedUnitIds?.Contains(id) == true) continue;
                    yield return unit;
                    processed.Add(id);
                }
            }

            // 2. 遍历父状态未处理单位 → 自动触发读时克隆
            foreach (var ownerUnit in Owner.GetAllUnits())
            {
                var id = ownerUnit.Id;
                if (processed.Contains(id)) continue;
                if (_removedUnitIds?.Contains(id) == true) continue;

                // 这里会自动克隆到本地
                yield return GetUnit(id);
            }
        }

        public void AddUnit(UnitState unit)
        {
            var id = unit.Id;
            if (!unit.Coords.IsInSize(Size))
            {
                Logger.Warn("单位{id}坐标{coords}超出地图大小{size}", id, unit.Coords, Size);
                return;
            }

            _removedUnitIds?.Remove(id);

            var existing = GetUnit(id);
            if (!existing.IsNull())
            {
                Logger.Warn("单位Id{id}已存在于数据上", id);
                return;
            }

            GetModifiedUnits()[id] = unit;
            _isModified = true;

            var coordsMap = GetModifiedCoords();
            var ownerUnitAtCoord = Owner.GetUnit(unit.Coords);
            if (!ownerUnitAtCoord.IsNull() && ownerUnitAtCoord.Id != id)
            {
                if (_modifiedUnits?.ContainsKey(ownerUnitAtCoord.Id) != true)
                    GetRemovedIds().Add(ownerUnitAtCoord.Id);
            }

            foreach (var (coord, uid) in coordsMap)
            {
                if (uid == id)
                {
                    coordsMap[coord] = 0;
                    break;
                }
            }

            coordsMap[unit.Coords] = id;
        }

        public void RemoveUnit(uint id)
        {
            var unit = GetUnit(id);
            if (unit.IsNull()) return;

            GetRemovedIds().Add(id);
            _modifiedUnits?.Remove(id);
            GetModifiedCoords()[unit.Coords] = 0;
            _isModified = true;
        }

        public void ModifyUnit(uint id, Action<UnitState> modifier)
        {
            if (_removedUnitIds?.Contains(id) == true) return;

            // GetUnit 已经保证返回本地克隆，直接修改即可
            var unit = (UnitState)GetUnit(id);
            if (unit.IsNull()) return;

            var oldCoords = unit.Coords;
            modifier(unit);

            if (unit.Coords != oldCoords)
            {
                var coordsMap = GetModifiedCoords();
                coordsMap[oldCoords] = 0;

                var occupant = GetUnit(unit.Coords);
                if (!occupant.IsNull() && occupant.Id != id)
                {
                    GetRemovedIds().Add(occupant.Id);
                    _modifiedUnits?.Remove(occupant.Id);
                }

                coordsMap[unit.Coords] = id;
            }

            _removedUnitIds?.Remove(id);
            _isModified = true;
        }

        public void ClearIfSnapshot()
        {
            if (_pooledUnits != null)
            {
                foreach (var u in _pooledUnits) UnitPool.Push(u);
                _pooledUnits.Clear();
            }

            _modifiedUnits?.Clear();
            _removedUnitIds?.Clear();
            _modifiedUnitAt?.Clear();
            _isModified = false;
        }

        public IUnitMapState GetSnapshot()
        {
            if (!_isModified) return this;
            return new UnitMapStateSnapshot(this);
        }
    }
}
