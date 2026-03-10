using System;
using System.Collections.Generic;
using Godot;

namespace TheBlockWar.Scripts.Games;

public class MapState : IMapState
{
    public Vector2I Size { get; }
    private readonly TileState[,] _tiles;
    public MapState(Vector2I size)
    {
        Size = size;
        _tiles = new TileState[size.X, size.Y];
    }
    public TileState GetTile(Vector2I coords)
    {
        return  _tiles[coords.X, coords.Y];
    }

    public void SetTile(Vector2I coords, in TileState state)
    {
        _tiles[coords.X, coords.Y] = state;
    }

    public void ModifyTile(Vector2I coords, Func<TileState, TileState> modifier)
    {
        var tile = GetTile(coords);
        _tiles[coords.X, coords.Y] = modifier(tile);
    }

    public IMapState GetSnapshot()
    {
        return new MapStateSnapshot(this);
    }
    
    
    public class MapStateSnapshot : IMapState
    {
        private IMapState _owner;
        public MapStateSnapshot(IMapState owner)
        {
            _owner = owner;
        }
        public Vector2I Size => _owner.Size;
        private Dictionary<Vector2I, TileState> _tiles = new();
        public TileState GetTile(Vector2I coords)
        {
            if (_tiles.TryGetValue(coords, out var tile)) return tile; 
            return _owner.GetTile(coords);
        }

        public void SetTile(Vector2I coords, in TileState state)
        {
            _tiles[coords] = state;
        }

        public void ModifyTile(Vector2I coords, Func<TileState, TileState> modifier)
        {
            var tile = GetTile(coords);
            _tiles[coords] = modifier(tile);
        }

        public IMapState GetSnapshot()
        {
            return new MapStateSnapshot(this);
        }
    }
}

