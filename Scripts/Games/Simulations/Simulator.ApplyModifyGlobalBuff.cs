using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        [Step([], [])]
        public ApplyModifyGlobalBuffResult ApplyModifyGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff, bool add)
        {
            EnterSimulation();
            var r = Main();
            ExitSimulation();
            return r;
            
            ApplyModifyGlobalBuffResult Main()
            {
                var r = obj.Steps.AddResult(new ApplyModifyGlobalBuffResult(
                    CurrentDeep, CurrentTeam, sourceUnitName, sourceBuff, groupMask, targetBuff, add));
                if (add)
                {
                    var index = obj.State.BuffGlobalState.AddGlobalBuff(sourceUnitName, sourceBuff, groupMask, targetBuff);
                    if (obj.State.BuffGlobalState.GlobalBuffs[index].Count == 1 && obj.ComplexSimulationRegion(3)) //如果第一次添加
                    {
                        foreach (var a in obj.State.UnitMapState.GetAllUnits())
                        {
                            if (a.Name != sourceUnitName && !a.IsDead && ((1 << a.Team) & groupMask) != 0)
                            {
                                obj.ApplyModifyUnitBuff(a.Coords, targetBuff, BuffOperation.AddBuff, 1, false);
                            }
                        }
                    }
                }
                else
                {
                    var index = obj.State.BuffGlobalState.RemoveGlobalBuff(sourceUnitName, sourceBuff, groupMask, targetBuff);
                    if (index == -1 && obj.ComplexSimulationRegion(3)) //如果移除
                    {
                        foreach (var a in obj.State.UnitMapState.GetAllUnits())
                        {
                            if (a.Name != sourceUnitName && !a.IsDead && ((1 << a.Team) & groupMask) != 0)
                            {
                                obj.ApplyModifyUnitBuff(a.Coords, targetBuff, BuffOperation.RemoveBuff, 1, false);
                            }
                        }
                    }
                }
                return r;
            }
        }
    }

    public static partial void ApplyModifyGlobalBuffReplayCore(IGameState state, ApplyModifyGlobalBuffResult result)
    {
        if (result.Add)
        {
            state.BuffGlobalState.AddGlobalBuff(result.SourceUnitName, result.SourceBuff, result.GroupMask, result.TargetBuff);
        }
        else
        {
            state.BuffGlobalState.RemoveGlobalBuff(result.SourceUnitName, result.SourceBuff, result.GroupMask, result.TargetBuff);
        }
    }

    public static partial void ApplyModifyGlobalBuffViewDraw(IGameView view, ReadOnlySimObject sim, ApplyModifyGlobalBuffResult result)
    {
    }
    public static partial GDTask ApplyModifyGlobalBuffViewRunAsync(IGameView view, ReadOnlySimObject sim, ApplyModifyGlobalBuffResult result)
    {
        return GDTask.CompletedTask;
    }
}