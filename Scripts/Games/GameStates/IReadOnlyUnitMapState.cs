using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IReadOnlyUnitMapState
{
    public IReadOnlyUnitMapState? Owner { get; }
    IEnumerable<IReadOnlyUnitState> GetAllUnits();
    bool HasUnit(uint id);
    bool HasUnit(Vector2I coords);
    /// <summary>
    /// 获取只读视图,注意在修改单位属性后请重新获取,因为修改后,快照会创建新的对象,而原本的对象是不变的
    /// </summary>
    IReadOnlyUnitState? GetUnit(uint id);
    /// <summary>
    /// 获取只读视图,注意在修改单位属性后请重新获取,因为修改后,快照会创建新的对象,而原本的对象是不变的
    /// </summary>
    IReadOnlyUnitState? GetUnit(Vector2I coords);
    
    IUnitMapState GetSnapshot();
}