using System;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IUnitMapState : IReadOnlyUnitMapState
{
    void AddUnit(UnitState unit, out IReadOnlyUnitState? renewedUnit);
    void RemoveUnit(uint id, out IReadOnlyUnitState? renewedUnit);
    /// <summary>
    /// 修改单位状态
    /// </summary>
    /// <returns>修改后的单位状态,如果是快照状态,则返回快照创建的克隆体</returns>
    void ModifyUnit(uint id, Action<UnitState> modifier, out IReadOnlyUnitState? renewedUnit); //out保证不要忘记更新单位
    void ClearIfSnapshot();
}