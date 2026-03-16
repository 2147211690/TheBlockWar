using System;
using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TileMapState : ITileMapState
{
    public IReadOnlyTileMapState? Owner => null;
    public Vector2I Size { get; }
    private readonly TileState[,] _tiles;
    public TileMapState(Vector2I size)
    {
        Size = size;
        _tiles = new TileState[size.X, size.Y];
        for (int x = 0; x < size.X; x++)
        {
            for (int y = 0; y < size.Y; y++)
            {
                _tiles[x, y] = TileState.Empty.Clone();
            }
        }
    }
    public IReadOnlyTileState GetTile(Vector2I coords)
    {
        return !coords.IsInSize(Size) ? TileState.Empty : _tiles[coords.X, coords.Y];
    }

    public bool IsEmpty(Vector2I coords)
    {
        return (GetTile(coords).Tag & TileTag.Empty) != 0;
    }

    public void ModifyTile(Vector2I coords, Action<TileState> modifier, out IReadOnlyTileState? renewedUnit)
    {
        if (!coords.IsInSize(Size))
        {
            renewedUnit = null;
            return;
        }
        modifier(_tiles[coords.X, coords.Y]);
        renewedUnit = _tiles[coords.X, coords.Y];
    }

    public void ClearIfSnapshot() {}

    public ITileMapState GetSnapshot()
    {
        return new TileMapStateSnapshot(this);
    }
    
    
    public class TileMapStateSnapshot : ITileMapState
    {
        public IReadOnlyTileMapState Owner { get; }
        public TileMapStateSnapshot(IReadOnlyTileMapState owner)
        {
            Owner = owner;
        }
        public Vector2I Size => Owner.Size;
        private List<TileState>? _popedTiles;
        private Dictionary<Vector2I, TileState>? _tiles = new();

        private static readonly TypedPool<TileState> TilePool = new(() => new TileState(), 10)
        {
        };
        private Dictionary<Vector2I, TileState> GetTiles() => _tiles ??= new();
        private List<TileState> GetPopedTiles() => _popedTiles ??= new();
        public IReadOnlyTileState GetTile(Vector2I coords)
        {
            if (_tiles?.TryGetValue(coords, out var tile) == true) return tile; 
            return Owner.GetTile(coords);
        }

        public bool IsEmpty(Vector2I coords) => (GetTile(coords).Tag & TileTag.Empty) != 0;

        public void ModifyTile(Vector2I coords, Action<TileState> modifier, out IReadOnlyTileState? renewedUnit)
        {
            if (_tiles?.TryGetValue(coords, out var tile) == true)
            {
                modifier(tile);
                renewedUnit = tile;
            }
            else
            {
                tile = Owner.GetTile(coords).CopyTo(TilePool.Pop());
                GetPopedTiles().Add(tile);
                modifier(tile);
                GetTiles()[coords] = tile;
                renewedUnit = tile;
            }
        }

        public void ClearIfSnapshot()
        {
            if (_popedTiles is not null) foreach (var tile in _popedTiles) TilePool.Push(tile);
            _popedTiles?.Clear();
            _tiles?.Clear();
        }

        public ITileMapState GetSnapshot()
        {
            return new TileMapStateSnapshot(this);
        }
    }
}

