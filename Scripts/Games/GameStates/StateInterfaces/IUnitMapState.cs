namespace TheBlockWar.Scripts.Games.GameStates;

public interface IUnitMapState : IReadOnlyUnitMapState, IGameDataState
{
    void AddUnit(UnitState unit);
    void RemoveUnit(long id);
    /// <summary>
    /// 修改单位状态
    /// </summary>
    /// <returns>修改后的单位状态,如果是快照状态,则返回快照创建的克隆体</returns>
    void ModifyUnit(long id, Action<UnitState> modifier); //保证不要忘记更新单位
}