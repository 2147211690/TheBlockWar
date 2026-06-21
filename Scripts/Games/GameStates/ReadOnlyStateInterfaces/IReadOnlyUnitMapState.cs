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
    /// 获取只读视图,注意在修改单位属性后请重新获取,因为修改后,快照会创建新的对象,而原本的对象是不变的
    /// </summary>
    IReadOnlyUnitState GetUnit(long id);
    /// <summary>
    /// 获取只读视图,注意在修改单位属性后请重新获取,因为修改后,快照会创建新的对象,而原本的对象是不变的
    /// </summary>
    IReadOnlyUnitState GetUnit(Vector2I coords);
    
}