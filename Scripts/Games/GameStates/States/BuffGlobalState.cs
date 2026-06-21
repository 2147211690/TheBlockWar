using System.Linq;
using System.Runtime.InteropServices;
using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public class BuffGlobalState : IBuffGlobalState
{
    public IReadOnlyList<GlobalBuffState> GlobalBuffs => _globalBuffs;
       private List<GlobalBuffState> _globalBuffs;
    public IEnumerable<string> GetTargetBuffs(string unitName, int group)
    {
        return GetTargetBuffs(_globalBuffs, unitName, group);
    }



    public int FindGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        return FindGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
    }

    private static IEnumerable<string> GetTargetBuffs(List<GlobalBuffState> list, string unitName, int group)
    {
        return list.Where(x => x.SourceUnitName != unitName && (x.GroupMask & (1 << group)) != 0).Select(x => x.TargetBuff);
    }
    public IBuffGlobalState GetSnapshot()
    {
        return new BuffGlobalStateSnapshot(this);
    }
    public BuffGlobalState()
    {
        _globalBuffs = new();
    }
    public BuffGlobalState(BuffGlobalStateData data)
    {
        _globalBuffs = new(data.GlobalBuffs);
    }
    public BuffGlobalStateData ToData()
    {
        return new BuffGlobalStateData()
        {
            GlobalBuffs = new(_globalBuffs)
        };
    }

    public int AddGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        return AddGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
    }

    public int RemoveGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        return RemoveGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
    }

    public void ClearIfSnapshot()
    {
    }


    private static int RemoveGlobalBuff(List<GlobalBuffState> list, string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        var i = FindGlobalBuff(list, sourceUnitName, sourceBuff, groupMask, targetBuff);
        if (i == -1)
            return -1;
        var span = CollectionsMarshal.AsSpan(list);
        span[i].Count--;
        if (span[i].Count == 0)
        {
            list.RemoveAt(i);
            return -1;
        }
        return i;
    }

    private static int FindGlobalBuff(List<GlobalBuffState> list, string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        int i = 0;
        for (; i < list.Count; i++)
            if (list[i].SourceUnitName == sourceUnitName && list[i].SourceBuff == sourceBuff && list[i].GroupMask == groupMask && list[i].TargetBuff == targetBuff)
                break;
        return i == list.Count ? -1 : i;
    }
    private static int AddGlobalBuff(List<GlobalBuffState> list, string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
    {
        var i = FindGlobalBuff(list, sourceUnitName, sourceBuff, groupMask, targetBuff);
        if (i == -1)
        {
            list.Add(new(sourceUnitName, sourceBuff, groupMask, targetBuff, 1));
            return list.Count - 1;
        }
        else
        {
            var span = CollectionsMarshal.AsSpan(list);
            span[i].Count++;
            return i;
        }
    }
    class BuffGlobalStateSnapshot : IBuffGlobalState
    {
        public IReadOnlyBuffGlobalState Owner { get; }
        public IReadOnlyList<GlobalBuffState> GlobalBuffs => _globalBuffs;
        
        private List<GlobalBuffState> _globalBuffs;
        public BuffGlobalStateSnapshot(IReadOnlyBuffGlobalState owner)
        {
            Owner = owner;
            _globalBuffs = new(Owner.GlobalBuffs.Count + 3);
            _globalBuffs.AddRange(Owner.GlobalBuffs);
        }
        
        public IEnumerable<string> GetTargetBuffs(string unitName, int group)
        {
            return BuffGlobalState.GetTargetBuffs(_globalBuffs, unitName, group);
        }

        public int FindGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
        {
            return BuffGlobalState.FindGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
        }

        public IBuffGlobalState GetSnapshot()
        {
            return new BuffGlobalStateSnapshot(this);
        }

        public BuffGlobalStateData ToData()
        {
            return new BuffGlobalStateData()
            {
                GlobalBuffs = new(_globalBuffs)
            };
        }

        public int AddGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
        {
            return BuffGlobalState.AddGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
        }

        public int RemoveGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff)
        {
            return BuffGlobalState.RemoveGlobalBuff(_globalBuffs, sourceUnitName, sourceBuff, groupMask, targetBuff);
        }

        public void ClearIfSnapshot()
        {
            _globalBuffs.Clear();
            _globalBuffs.Capacity = Owner.GlobalBuffs.Count + 3;
            _globalBuffs.AddRange(Owner.GlobalBuffs);
        }
    }
}