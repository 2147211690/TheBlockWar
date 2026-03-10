using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public class GameState : IGameState
{
    public IMapState MapState { get; }
    private Dictionary<uint, UnitState> _units = new();
    private Dictionary<Vector2I, uint> _unitAt = new();

    public IEnumerable<UnitState> GetAllUnits()
    {
        return _units.Values;
    }

    public UnitState? GetUnit(uint id)
    {
        return _units.GetValueOrDefault(id);
    }

    public UnitState? GetUnit(Vector2I coords)
    {
        return _unitAt.TryGetValue(coords, out var id) ? _units.GetValueOrDefault(id) : null;
    }

    public void RemoveUnit(uint id)
    {
        if (!_units.Remove(id, out var unit)) return;
        _unitAt.Remove(unit.Coords);
    }

    public void ModifyUnit(uint id, Func<UnitState, UnitState> modifier)
    {
        if (!_units.Remove(id, out var oldUnit)) return;
        _unitAt.Remove(oldUnit.Coords);
        var newUnit = modifier(oldUnit);
        newUnit.Id = id;
        _units[id] = newUnit;
        _unitAt.Add(newUnit.Coords, id);
    }

    public void AddUnit(in UnitState unit)
    {
        if (_units.ContainsKey(unit.Id))
        {
            GD.PrintErr($"Unit with id {unit.Id} is already added");
            RemoveUnit(unit.Id);
        }
        _units[unit.Id] = unit;
        _unitAt[unit.Coords] = unit.Id;
    }
    public IGameState GetSnapshot()
    {
        return new GameStateSnapshot(this);
    }
    public GameState(Vector2I mapSize)
    {
        MapState = new MapState(mapSize);
    }


    /// <summary>
    /// 游戏状态快照 - 实现写时复制（Copy-on-Write）模式
    /// 读取时优先查看本地修改，未修改则回退到原状态
    /// </summary>
    public class GameStateSnapshot : IGameState
    {
        private readonly IGameState _owner;
        // 存储被修改的单位：(新状态, 是否存在于地图中)
        private readonly Dictionary<uint, (UnitState unit, bool inMap)> _dirtyUnits = new();
        // 本地坐标到ID的映射（仅包含被修改的坐标）
        private readonly Dictionary<Vector2I, uint> _dirtyUnitAt = new();
        // 记录被删除的坐标，用于遮挡原状态的查询
        private readonly HashSet<Vector2I> _removedCoords = new();

        public GameStateSnapshot(IGameState owner)
        {
            _owner = owner;
            MapState = owner.MapState.GetSnapshot();
        }

        public IMapState MapState { get; }

        public UnitState? GetUnit(uint id)
        {
            if (_dirtyUnits.TryGetValue(id, out var dirty))
            {
                return dirty.inMap ? dirty.unit : null;
            }
            return _owner.GetUnit(id);
        }

        public UnitState? GetUnit(Vector2I coords)
        {
            if (_dirtyUnitAt.TryGetValue(coords, out var dirtyId))
            {
                return _dirtyUnits.TryGetValue(dirtyId, out var dirty) && dirty.inMap 
                    ? dirty.unit 
                    : null;
            }
            
            if (_removedCoords.Contains(coords))
            {
                return null;
            }
            
            return _owner.GetUnit(coords);
        }

        public void RemoveUnit(uint id)
        {
            var current = GetUnit(id);
            if (current == null) return;

            _dirtyUnits[id] = (default, false);
            _removedCoords.Add(current.Value.Coords);
            _dirtyUnitAt.Remove(current.Value.Coords);
        }

        public void AddUnit(in UnitState unit)
        {
            var existing = GetUnit(unit.Coords);
            if (existing != null)
            {
                RemoveUnit(existing.Value.Id);
            }

            if (_dirtyUnits.ContainsKey(unit.Id))
            {
                var oldDirty = _dirtyUnits[unit.Id];
                if (oldDirty.inMap)
                {
                    _removedCoords.Add(oldDirty.unit.Coords);
                    _dirtyUnitAt.Remove(oldDirty.unit.Coords);
                }
            }
            else
            {
                var oldUnit = _owner.GetUnit(unit.Id);
                if (oldUnit != null)
                {
                    _removedCoords.Add(oldUnit.Value.Coords);
                }
            }

            _dirtyUnits[unit.Id] = (unit, true);
            _dirtyUnitAt[unit.Coords] = unit.Id;
            _removedCoords.Remove(unit.Coords);
        }

        public void ModifyUnit(uint id, Func<UnitState, UnitState> modifier)
        {
            var oldUnit = GetUnit(id);
            if (oldUnit == null) return;
            var unit = modifier(oldUnit.Value);
            unit.Id = id;
            if (oldUnit.Value.Coords != unit.Coords)
            {
                _removedCoords.Add(oldUnit.Value.Coords);
                _dirtyUnitAt.Remove(oldUnit.Value.Coords);
                _dirtyUnitAt[unit.Coords] = id;
                _removedCoords.Remove(unit.Coords);
            }

            _dirtyUnits[id] = (unit, true);
        }

        public IGameState GetSnapshot()
        {
            return new GameStateSnapshot(this);
        }

        /// <summary>
        /// 获取所有单位（快照视图）
        /// 合并原状态 + 本地修改 - 本地删除
        /// </summary>
        public IEnumerable<UnitState> GetAllUnits()
        {
            // 1. 先收集所有需要处理的ID
            var allIds = new HashSet<uint>();
            
            // 添加原状态的所有单位ID
            foreach (var unit in _owner.GetAllUnits())
            {
                allIds.Add(unit.Id);
            }
            
            // 添加本地修改过的单位ID
            foreach (var id in _dirtyUnits.Keys)
            {
                allIds.Add(id);
            }

            // 2. 通过GetUnit获取每个ID的最终状态（自动处理脏标记）
            foreach (var id in allIds)
            {
                var unit = GetUnit(id);
                if (unit != null)
                {
                    yield return unit.Value;
                }
            }
        }
    }
}