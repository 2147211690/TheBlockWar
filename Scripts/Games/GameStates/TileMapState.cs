using System;
using System.Collections.Generic;
using Godot;

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
        if (coords.X < 0 || coords.X >= Size.X || coords.Y < 0 || coords.Y >= Size.Y) return TileState.Empty;
        return _tiles[coords.X, coords.Y];
    }
    
    public void ModifyTile(Vector2I coords, Action<TileState> modifier)
    {
        modifier(_tiles[coords.X, coords.Y]);
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
        private Dictionary<Vector2I, TileState>? _tiles = new();
        private Dictionary<Vector2I, TileState> GetTiles() => _tiles ??= new();
        public IReadOnlyTileState GetTile(Vector2I coords)
        {
            if (_tiles?.TryGetValue(coords, out var tile) == true) return tile; 
            return Owner.GetTile(coords);
        }
        
        public void ModifyTile(Vector2I coords, Action<TileState> modifier)
        {
            if (_tiles?.TryGetValue(coords, out var tile) == true)
            {
                modifier(tile);
            }
            else
            {
                tile = Owner.GetTile(coords).Clone();
                modifier(tile);
                GetTiles()[coords] = tile;
            }
        }

        public void ClearIfSnapshot()
        {
            _tiles?.Clear();
        }

        public ITileMapState GetSnapshot()
        {
            return new TileMapStateSnapshot(this);
        }
    }
}

