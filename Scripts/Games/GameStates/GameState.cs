namespace TheBlockWar.Scripts.Games.GameStates;

public class GameState : IGameState
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public IReadOnlyGameState? Owner => null;
    public ITileMapState TileMapState { get; }
    public IUnitMapState UnitMapState { get; }
    public TeamState TeamState { get; }
    
    IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
    IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
    private Dictionary<string, ISkill[]> _unitSkillMap;
    public IReadOnlyList<ISkill> GetUnitValidSkills(string name)
    {
        //TODO: 根据单位名称获取有效技能
        return _unitSkillMap.TryGetValue(name, out var value) ? value : [];
    }

    public IGameState GetSnapshot() => new GameStateSnapshot(this);

    public GameState(Vector2I mapSize, Dictionary<string, ISkill[]> unitSkillMap, int[,] teamRelations)
    {
        _unitSkillMap = unitSkillMap;
        TileMapState = new TileMapState(mapSize);
        UnitMapState = new UnitMapState(mapSize);
        TeamState = new TeamState(teamRelations);
    }
    public void ClearIfSnapshot(){}
    /// <summary>
    /// 增量游戏状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// </summary>
    public sealed class GameStateSnapshot : IGameState
    {
        // 父状态引用 - 未修改数据的回退源
        public IReadOnlyGameState Owner { get; }

        public ITileMapState TileMapState { get; }
        public IUnitMapState UnitMapState { get; }
        public TeamState TeamState => Owner.TeamState;

        IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
        IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
        public GameStateSnapshot(IReadOnlyGameState owner)
        {
            // 只能是GameState或其快照
            Owner = owner;
            
            TileMapState = owner.TileMapState.GetSnapshot();
            UnitMapState = owner.UnitMapState.GetSnapshot();
        }

        public IReadOnlyList<ISkill> GetUnitValidSkills(string name) => Owner.GetUnitValidSkills(name);
        public void ClearIfSnapshot()
        {
            TileMapState.ClearIfSnapshot();
            UnitMapState.ClearIfSnapshot();
        }

        public IGameState GetSnapshot()
        {
            return new GameStateSnapshot(this);
        }
    }
}