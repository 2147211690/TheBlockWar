using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyBuffGlobalState : IReadOnlyGameDataState<IBuffGlobalState, BuffGlobalStateData>
{
    IReadOnlyList<GlobalBuffState> GlobalBuffs { get; }
    IEnumerable<string> GetTargetBuffs(string unitName, int group);
    int FindGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff);
}