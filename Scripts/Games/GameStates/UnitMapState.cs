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

    public IReadOnlyUnitMapState? Owner => null;
    public IEnumerable<IReadOnlyUnitState> GetAllUnits() => _units.Values;

    public bool HasUnit(uint id)
    {
        return _units.ContainsKey(id);
    }

    public bool HasUnit(Vector2I coords)
    {
        return _unitAt.ContainsKey(coords);
    }

    public IReadOnlyUnitState? GetUnit(uint id) => _units.GetValueOrDefault(id);

    public IReadOnlyUnitState? GetUnit(Vector2I coords) => 
        _unitAt.TryGetValue(coords, out var id) ? _units.GetValueOrDefault(id) : null;

    public IUnitMapState GetSnapshot()
    {
        return new UnitMapStateSnapshot(this);
    }

    public void AddUnit(UnitState unit)
    {
        if (_units.ContainsKey(unit.Id))
        {
            Logger.Warn("单位Id{id}已经存在于数据上", unit.Id);
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
        unitState.Id = id;//确保ID一致
        
        if (oldCoords != unitState.Coords)
        {
            _unitAt.Remove(oldCoords);
            _unitAt[unitState.Coords] = id;
        }
    }

    public void ClearIfSnapshot(){}

    /// <summary>
    /// 单位地图状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// </summary>
    public sealed class UnitMapStateSnapshot : IUnitMapState
    {
        
        // 父状态引用 - 未修改数据的回退源
        public IReadOnlyUnitMapState Owner { get; }
        
        // 增量存储：仅包含相对于owner的变更
        private Dictionary<uint, UnitState>? _modifiedUnits;      // 修改/新增的单位
        private HashSet<uint>? _removedUnitIds;                    // 删除的单位ID
        private Dictionary<Vector2I, uint>? _modifiedUnitAt;       // 坐标变更（仅包含变更的坐标）
        private List<UnitState>? _pooledUnits;
        //池
        private static readonly TypedPool<UnitState> UnitPool = new(() => new UnitState(), 1)
        {
        };
        
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
            var ownerId = Owner.GetUnit(coords)?.Id ?? 0u;
            if (ownerId != 0 && _removedUnitIds?.Contains(ownerId) == true)
                return false;

            return Owner.HasUnit(coords);
        }

        public IReadOnlyUnitState? GetUnit(uint id)
        {
            // 1. 检查是否被删除
            if (_removedUnitIds?.Contains(id) == true)
                return null;

            // 2. 检查本地修改
            if (_modifiedUnits?.TryGetValue(id, out var modified) == true)
                return modified;

            // 3. 回退到父状态
            return Owner.GetUnit(id);
        }

        public IReadOnlyUnitState? GetUnit(Vector2I coords)
        {
            // 1. 检查坐标是否被本地修改（包括删除导致的空位）
            if (_modifiedUnitAt?.TryGetValue(coords, out var localId) == true)
            {
                // 如果是0表示该坐标被清空
                if (localId == 0) return null;
                return _modifiedUnits?.GetValueOrDefault(localId) 
                    ?? Owner.GetUnit(localId);
            }

            // 2. 检查该坐标的单位是否被删除
            var ownerUnit = Owner.GetUnit(coords);
            if (ownerUnit != null && _removedUnitIds?.Contains(ownerUnit.Id) == true)
                return null;

            return ownerUnit;
        }

        public IEnumerable<IReadOnlyUnitState> GetAllUnits()
        {
            // 合并父状态和本地修改的迭代器
            var processedIds = new HashSet<uint>();
            
            // 1. 先遍历父状态（排除被删除的）
            foreach (var unit in Owner.GetAllUnits())
            {
                if (_removedUnitIds?.Contains(unit.Id) == true) continue;
                
                // 如果被修改，使用修改后的版本
                if (_modifiedUnits?.TryGetValue(unit.Id, out var modified) == true)
                {
                    yield return modified;
                    processedIds.Add(unit.Id);
                }
                else
                {
                    yield return unit;
                    processedIds.Add(unit.Id);
                }
            }

            // 2. 补充新增的单位（父状态中不存在的）
            if (_modifiedUnits != null)
            {
                foreach (var (id, unit) in _modifiedUnits)
                {
                    if (!processedIds.Contains(id))
                        yield return unit;
                }
            }
        }

        public void AddUnit(UnitState unit)
        {
            var id = unit.Id;
            
            // 从删除集合中移除（如果存在）
            _removedUnitIds?.Remove(id);
            
            // 检查是否已存在（可能是修改）
            var existing = GetUnit(id);
            
            GetModifiedUnits()[id] = unit;
            _isModified = true;
            
            // 更新坐标映射
            var coordsMap = GetModifiedCoords();
            
            // 如果该坐标在父状态中有其他单位，需要处理冲突
            var ownerUnitAtCoord = Owner.GetUnit(unit.Coords);
            if (ownerUnitAtCoord != null && ownerUnitAtCoord.Id != id)
            {
                // 标记原位置单位被移除（如果未被修改）
                if (_modifiedUnits?.ContainsKey(ownerUnitAtCoord.Id) != true)
                {
                    GetRemovedIds().Add(ownerUnitAtCoord.Id);
                }
            }
            
            // 清除该坐标上的旧映射（如果有）
            foreach (var (coord, uid) in coordsMap)
            {
                if (uid == id) 
                {
                    coordsMap[coord] = 0; // 标记为空
                    break;
                }
            }
            
            coordsMap[unit.Coords] = id;
        }

        public void RemoveUnit(uint id)
        {
            var unit = GetUnit(id);
            if (unit == null) return;

            // 标记为删除
            GetRemovedIds().Add(id);
            
            // 从修改集合中移除（如果存在）
            _modifiedUnits?.Remove(id);
            
            // 标记坐标为空
            GetModifiedCoords()[unit.Coords] = 0;
            _isModified = true;
        }

        public void ModifyUnit(uint id, Action<UnitState> modifier)
        {
            // 1. 检查是否被删除
            if (_removedUnitIds?.Contains(id) == true) return;
            Vector2I oldCoords;
            IReadOnlyUnitState newUnit;
            // 2. 检查本地修改
            if (_modifiedUnits?.TryGetValue(id, out var modified) == true)
            {
                //本地找到直接原地修改
                oldCoords = modified.Coords;
                modifier(modified);
                modified.Id = id;
                newUnit = modified;
            }
            else
            {
                //本地未找到，从父状态克隆
                var ownerUnit = Owner.GetUnit(id);
                if (ownerUnit == null) return;
                var clone = ownerUnit.CopyTo(UnitPool.Pop());
                GetPooledUnits().Add(clone);
                //var clone = ownerUnit.Clone();
                oldCoords = clone.Coords;
                modifier(clone);
                clone.Id = id;
                GetModifiedUnits()[id] = clone;
                newUnit = clone;
            }
            
            // 如果坐标改变，需要更新坐标映射
            if (newUnit.Coords != oldCoords)
            {
                var coordsMap = GetModifiedCoords();
                
                // 旧坐标清空
                coordsMap[oldCoords] = 0;
                
                // 检查新坐标是否被占用
                var occupant = GetUnit(newUnit.Coords);
                if (occupant != null && occupant.Id != id)
                {
                    // 移除占用者
                    GetRemovedIds().Add(occupant.Id);
                    _modifiedUnits?.Remove(occupant.Id);
                }
                
                coordsMap[newUnit.Coords] = id;
            }

            //GetModifiedUnits()[id] = newUnit;
            _removedUnitIds?.Remove(id); // 确保不在删除集合中
            _isModified = true;
        }

        public void ClearIfSnapshot()
        {
            if (_pooledUnits is not null) foreach (var unit in _pooledUnits) UnitPool.Push(unit);
            _pooledUnits?.Clear();
            _modifiedUnitAt?.Clear();
            _modifiedUnits?.Clear();
            _removedUnitIds?.Clear();
            _isModified = false;
        }

        public IUnitMapState GetSnapshot()
        {
            // 优化：如果当前快照未被修改，直接返回自身或父状态
            if (!_isModified)
                return this;

            // 创建新的快照层级
            return new UnitMapStateSnapshot(this);
        }
    }
}
