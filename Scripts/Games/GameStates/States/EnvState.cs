using System.Linq;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.GameStates;

public class EnvState : IEnvState
{
    public IEnumerable<EnvStepState> EnvSteps => _envSteps;

    private List<EnvStepState> _envSteps;
    private MultiDictionary<Vector2I, EnvStepState> _envStepsByCoords;

    public EnvState(EnvStateData data)
    {
        _envStepsByCoords = new(false);
        _envSteps = data.EnvSteps.ToList();
        Sort(_envSteps);
        foreach (var envStep in _envSteps)
        {
            _envStepsByCoords.Add(envStep.Coords, envStep);
        }
    }
    
    public EnvState()
    {
        _envSteps = new();
        _envStepsByCoords = new(false);
    }

    public IEnumerable<EnvStepState> GetEnvSteps(Vector2I coords)
    {
        return _envStepsByCoords[coords];
    }
    public void AddEnvStep(Vector2I coords, int priority, ApplyStep step)
    {
        _envSteps.Add(new EnvStepState(coords, priority, step));
        Sort(_envSteps);
        _envStepsByCoords.Add(coords, new EnvStepState(coords, priority, step));
    }

    public void ClearEnvSteps()
    {
        _envStepsByCoords.Clear();
        _envSteps.Clear();
    }
    public EnvStateData ToData()
    {
        return new EnvStateData
        {
            EnvSteps = _envSteps.ToList(),
        };
    }

    public IEnvState GetSnapshot()
    {
        return new EnvStateSnapshot(this);
    }

    public void ClearIfSnapshot()
    {
    }
    private static void Sort(List<EnvStepState> envSteps)
    {
        envSteps.Sort((a, b) => a.Priority.CompareTo(b.Priority));
    }



    /// <summary>
    /// 环境步骤快照 - 写时复制（COW）实现
    /// 内存优化：仅记录变更，未变更数据保持对父状态（Owner）的引用
    /// 变更范围：EnvStepState 为不可变 record，仅支持新增与清空，因此增量只需记录
    /// 1) 是否清空过（一旦清空，owner 的步骤全部失效）
    /// 2) 新增的步骤（含按坐标索引）
    /// 合并读取保证 EnvSteps 仍按优先级全局升序
    /// </summary>
    public sealed class EnvStateSnapshot : IEnvState
    {
        // 父状态 - 未修改数据的回退源
        public IReadOnlyEnvState Owner { get; }

        // 增量存储：仅包含相对于 owner 的变更
        private List<EnvStepState>? _addedSteps; // 新增的步骤（始终按优先级升序维护）
        private Dictionary<Vector2I, List<EnvStepState>>? _addedByCoords; // 新增步骤按坐标索引
        private bool _cleared; // 是否被清空过

        public EnvStateSnapshot(IReadOnlyEnvState owner)
        {
            // 只能是 EnvState 或其快照
            Owner = owner;
        }

        private List<EnvStepState> GetAddedSteps() =>
            _addedSteps ??= new List<EnvStepState>();

        private Dictionary<Vector2I, List<EnvStepState>> GetAddedByCoords() =>
            _addedByCoords ??= new Dictionary<Vector2I, List<EnvStepState>>();

        /// <summary>
        /// 合并枚举：将 owner 与本地新增两条有序序列归并，保持优先级全局升序
        /// </summary>
        public IEnumerable<EnvStepState> EnvSteps
        {
            get
            {
                if (_cleared)
                {
                    if (_addedSteps != null)
                        foreach (var step in _addedSteps)
                            yield return step;
                    yield break;
                }

                if (_addedSteps is null or { Count: 0 })
                {
                    foreach (var step in Owner.EnvSteps)
                        yield return step;
                    yield break;
                }

                using var ownerEnum = Owner.EnvSteps.GetEnumerator();
                using var addedEnum = _addedSteps.GetEnumerator();
                var hasOwner = ownerEnum.MoveNext();
                var hasAdded = addedEnum.MoveNext();
                while (hasOwner || hasAdded)
                {
                    if (hasOwner && (!hasAdded || ownerEnum.Current.Priority <= addedEnum.Current.Priority))
                    {
                        yield return ownerEnum.Current;
                        hasOwner = ownerEnum.MoveNext();
                    }
                    else
                    {
                        yield return addedEnum.Current;
                        hasAdded = addedEnum.MoveNext();
                    }
                }
            }
        }

        public IEnumerable<EnvStepState> GetEnvSteps(Vector2I coords)
        {
            if (!_cleared)
            {
                foreach (var step in Owner.GetEnvSteps(coords))
                    yield return step;
            }
            if (_addedByCoords != null && _addedByCoords.TryGetValue(coords, out var added))
            {
                foreach (var step in added)
                    yield return step;
            }
        }

        public void AddEnvStep(Vector2I coords, int priority, ApplyStep step)
        {
            var envStep = new EnvStepState(coords, priority, step);

            var addedSteps = GetAddedSteps();
            addedSteps.Add(envStep);
            Sort(addedSteps);

            var addedByCoords = GetAddedByCoords();
            if (!addedByCoords.TryGetValue(coords, out var list))
            {
                list = new List<EnvStepState>();
                addedByCoords[coords] = list;
            }
            list.Add(envStep);
        }

        public void ClearEnvSteps()
        {
            _cleared = true;
            _addedSteps?.Clear();
            _addedByCoords?.Clear();
        }

        public void ClearIfSnapshot()
        {
            _cleared = false;
            _addedSteps?.Clear();
            _addedByCoords?.Clear();
        }

        public IEnvState GetSnapshot()
        {
            return new EnvStateSnapshot(this);
        }

        public EnvStateData ToData()
        {
            return new EnvStateData { EnvSteps = EnvSteps.ToList() };
        }
    }


}