namespace TheBlockWar.Scripts.Games.GameStates;

public interface IBuffGlobalState : IReadOnlyBuffGlobalState, IGameDataState
{
    int AddGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff);
    int RemoveGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff);
}