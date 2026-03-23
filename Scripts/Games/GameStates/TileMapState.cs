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

    public void ModifyTile(Vector2I coords, Action<TileState> modifier)
    {
        if (!coords.IsInSize(Size)) return;

        modifier(_tiles[coords.X, coords.Y]);
    }

    public void ClearIfSnapshot()
    {
    }

    public ITileMapState GetSnapshot()
    {
        return new TileMapStateSnapshot(this);
    }


    public class TileMapStateSnapshot : ITileMapState
    {
        // 父状态
        public IReadOnlyTileMapState Owner { get; }
        public Vector2I Size => Owner.Size;
        // 核心：预分配 二维数组（全量TileState，Size大小）
        private TileState[,] _tileArray;
        // 标记：哪些坐标已经从Owner克隆初始化过（第一次读取标记）
        private HashSet<Vector2I> _initializedCoords = new();
        
        public TileMapStateSnapshot(IReadOnlyTileMapState owner)
        {
            Owner = owner;

            // 关键：构造时 提前生成Size数量的空TileState（二维数组）
            var size = owner.Size;
            _tileArray = new TileState[size.X, size.Y];
            // 预分配池对象（一次性拿够，避免运行时分配）
            for (int x = 0; x < size.X; x++)
            {
                for (int y = 0; y < size.Y; y++)
                {
                    _tileArray[x, y] = TileState.Empty.Clone();
                }
            }
        }

        /// <summary>
        /// 核心：永远返回本地数组中的Tile，绝不返回Owner
        /// 第一次读取：自动从Owner克隆到本地
        /// </summary>
        public IReadOnlyTileState GetTile(Vector2I coords)
        {
            // 越界判断（安全）
            if (!coords.IsInSize(Size))
                return TileState.Empty;

            int x = coords.X;
            int y = coords.Y;

            // 第一次访问：从Owner克隆到本地数组
            if (!_initializedCoords.Contains(coords))
            {
                Owner.GetTile(coords).CopyTo(_tileArray[x, y]);
                _initializedCoords.Add(coords);
            }

            // 永远返回本地数组的对象
            return _tileArray[x, y];
        }

        public bool IsEmpty(Vector2I coords)
        {
            // 内部调用GetTile，保证走本地克隆
            return (GetTile(coords).Tag & TileTag.Empty) != 0;
        }

        /// <summary>
        /// 修改永远只改本地数组，绝对安全
        /// </summary>
        public void ModifyTile(Vector2I coords, Action<TileState> modifier)
        {
            if (!coords.IsInSize(Size)) return;

            // 确保已经克隆到本地（第一次修改也会自动初始化）
            var tile = (TileState)GetTile(coords);
            modifier(tile);
        }

        public void ClearIfSnapshot()
        {
            // 清空数组与标记;
            _initializedCoords.Clear();
        }

        public ITileMapState GetSnapshot()
        {
            return new TileMapStateSnapshot(this);
        }

    }
}
