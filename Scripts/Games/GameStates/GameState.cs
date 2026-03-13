using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games.GameStates;

public class GameState : IGameState
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public ITileMapState TileMapState { get; }
    public IUnitMapState UnitMapState { get; }
    IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
    IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
    
    public IGameState GetSnapshot() => new GameStateSnapshot(this);

    public GameState(Vector2I mapSize)
    {
        TileMapState = new TileMapState(mapSize);
        UnitMapState = new UnitMapState();
    }

    /// <summary>
    /// 增量游戏状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// </summary>
    public sealed class GameStateSnapshot : IGameState
    {
        // 父状态引用 - 未修改数据的回退源
        private readonly IReadOnlyGameState _owner;
        
        public ITileMapState TileMapState { get; }
        public IUnitMapState UnitMapState { get; }

        IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
        IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
        public GameStateSnapshot(IReadOnlyGameState owner)
        {
            // 只能是GameState或其快照
            _owner = owner;
            
            TileMapState = owner.TileMapState.GetSnapshot();
            UnitMapState = owner.UnitMapState.GetSnapshot();
        }
        
        public IGameState GetSnapshot()
        {
            return new GameStateSnapshot(this);
        }
    }
}