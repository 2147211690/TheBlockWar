using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public class TilesState : ITilesState
{
    public Vector2I Size { get; }
    private readonly TileState[,] _tiles;
    public TilesState(Vector2I size)
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
        return _tiles[coords.X, coords.Y];
    }
    
    public void ModifyTile(Vector2I coords, Action<TileState> modifier)
    {
        modifier(_tiles[coords.X, coords.Y]);
    }

    public ITilesState GetSnapshot()
    {
        return new TilesStateSnapshot(this);
    }
    
    
    public class TilesStateSnapshot : ITilesState
    {
        private ITilesState _owner;
        public TilesStateSnapshot(ITilesState owner)
        {
            _owner = owner;
        }
        public Vector2I Size => _owner.Size;
        private Dictionary<Vector2I, TileState> _tiles = new();
        public IReadOnlyTileState GetTile(Vector2I coords)
        {
            if (_tiles.TryGetValue(coords, out var tile)) return tile; 
            return _owner.GetTile(coords);
        }
        
        public void ModifyTile(Vector2I coords, Action<TileState> modifier)
        {
            if (_tiles.TryGetValue(coords, out var tile))
            {
                modifier(tile);
            }
            else
            {
                tile = _owner.GetTile(coords).Clone();
                modifier(tile);
                _tiles[coords] = tile;
            }
        }

        public ITilesState GetSnapshot()
        {
            return new TilesStateSnapshot(this);
        }
    }
}

