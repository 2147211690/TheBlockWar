namespace TheBlockWar.Scripts.Games.GameStates;

public struct GlobalBuffState
{
    public GlobalBuffState(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff, int count)
    {
        SourceUnitName = sourceUnitName;
        SourceBuff = sourceBuff;
        GroupMask = groupMask;
        TargetBuff = targetBuff;
        Count = count;
    }
    public string SourceUnitName { get; set; }
    public string SourceBuff { get; set; }
    public int GroupMask { get; set; }
    public string TargetBuff { get; set; }
    public int Count { get; set; }
}