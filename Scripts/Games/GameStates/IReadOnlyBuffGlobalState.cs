using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyBuffGlobalState
{
    IReadOnlyList<GlobalBuffState> GlobalBuffs { get; }
    IEnumerable<string> GetTargetBuffs(string unitName, int group);
    int FindGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff);
    IBuffGlobalState GetSnapshot();
    BuffGlobalStateData ToData();
}