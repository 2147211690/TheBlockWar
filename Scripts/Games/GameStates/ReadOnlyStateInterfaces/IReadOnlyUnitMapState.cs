using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitMapState : IReadOnlyGameDataState<IUnitMapState, UnitMapStateData>
{
    IReadOnlyUnitMapState? Owner { get; }
    Vector2I Size { get; }
    IEnumerable<IReadOnlyUnitState> GetAllUnits();
    bool HasUnit(long id);
    bool HasUnit(Vector2I coords);
    /// <summary>
    /// 获取只读视图
    /// </summary>
    IReadOnlyUnitState GetUnit(long id);
    /// <summary>
    /// 获取只读视图
    /// </summary>
    IReadOnlyUnitState GetUnit(Vector2I coords);
    
}