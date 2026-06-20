using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Simulations;

partial class Simulator
{
    extension(SimObject obj)
    {
        public void ApplyModifyGlobalBuff(string sourceUnitName, string sourceBuff, int groupMask, string targetBuff, bool add)
        {
            EnterSimulation();
            Main();
            ExitSimulation();
            void Main()
            {
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
            }
        }
    }
}