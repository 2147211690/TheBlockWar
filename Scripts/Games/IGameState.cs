using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public interface IGameState : IReadOnlyGameState
{
    new ITilesState TilesState { get; }
    void RemoveUnit(uint id);
    //TODO:可能不会在外部创建对象传递进来,应该在内部创建,并使用对象池优化
    void AddUnit(UnitState unit);
    void ModifyUnit(uint id, Action<UnitState> modifier);
    
}