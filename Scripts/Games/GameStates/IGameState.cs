using System;

namespace TheBlockWar.Scripts.Games.GameStates;

public interface IGameState : IReadOnlyGameState
{
    new ITileMapState TileMapState { get; }
    new IUnitMapState UnitMapState { get; }   
    
}