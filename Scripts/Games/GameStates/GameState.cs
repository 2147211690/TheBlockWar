using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Units;
using TheBlockWar.Scripts.Tools;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games.GameStates;

public class GameState : IGameState
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    public IDictionary<long, SchedulerEvent> Events => _events;
    IReadOnlyDictionary<long, SchedulerEvent> IReadOnlyGameState.Events => _events;
    public Vector2I Size { get; }
    public DefaultIdGenerator IdGenerator { get; }
    public DateTime BaseTime { get; }
    public IReadOnlyGameState? Owner => null;
    public ITileMapState TileMapState { get; }
    public IUnitMapState UnitMapState { get; }
    public TeamState TeamState { get; }
    
    IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
    IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
    private Dictionary<string, ISkill[]> _unitSkillMap;
    private Dictionary<long, SchedulerEvent> _events;

    public IReadOnlyList<ISkill> GetUnitValidSkills(string unitName)
    {
        //TODO: 根据单位名称获取有效技能
        return _unitSkillMap.TryGetValue(unitName, out var value) ? value : [];
    }

    public IGameState GetSnapshot() => new GameStateSnapshot(this);

    public GameState(GameStateData data)
    {
        _unitSkillMap = new Dictionary<string, ISkill[]>();
        _events = new();
        foreach (var eventItem in data.Events)
        {
            _events[eventItem.Id] = eventItem;
        }
        Size = data.Size;
        BaseTime = data.BaseTime;
        IdGenerator = new DefaultIdGenerator(new IdGeneratorOptions()
        {
            WorkerId = IdGeneratorConfig.WorkerId,
            BaseTime = data.BaseTime,
        });
        TileMapState = new TileMapState(data.TileMapState);
        UnitMapState = new UnitMapState(data.UnitMapState);
        TeamState = new TeamState(data.TeamState);
    }

    public GameState(Vector2I mapSize, Dictionary<string, ISkill[]> unitSkillMap, int[,] teamRelations)
    {
        _unitSkillMap = unitSkillMap;
        _events = new();
        Size = mapSize;
        BaseTime = DateTime.UtcNow;
        IdGenerator = new DefaultIdGenerator(new IdGeneratorOptions()
        {
            WorkerId = IdGeneratorConfig.WorkerId,
            BaseTime = BaseTime,
        });
        TileMapState = new TileMapState(mapSize);
        UnitMapState = new UnitMapState(mapSize);
        TeamState = new TeamState(teamRelations);
    }
    public void ClearIfSnapshot(){}
    public GameStateData ToData()
    {
        return new GameStateData
        {
            Size = Size,
            BaseTime = BaseTime,
            TileMapState = TileMapState.ToData(),
            UnitMapState = UnitMapState.ToData(),
            TeamState = TeamState.ToData(),
            Events = _events.Values.ToList(),
        };
    }
    /// <summary>
    /// 增量游戏状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// </summary>
    public sealed class GameStateSnapshot : IGameState
    {
        // 父状态引用 - 未修改数据的回退源
        IReadOnlyDictionary<long, SchedulerEvent>? IReadOnlyGameState.Events => null;
        IDictionary<long, SchedulerEvent>? IGameState.Events => null;

        public Vector2I Size => Owner.Size;
        public DefaultIdGenerator IdGenerator => Owner.IdGenerator;
        public DateTime BaseTime => Owner.BaseTime;
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
        public GameStateData ToData()
        {
            return new GameStateData
            {
                BaseTime = BaseTime,
                TileMapState = TileMapState.ToData(),
                UnitMapState = UnitMapState.ToData(),
                TeamState = TeamState.ToData(),
                Events = Owner.Events?.Values.ToList() ?? [],
                Size = Size,
            };
        }

        public IReadOnlyList<ISkill> GetUnitValidSkills(string unitName) => Owner.GetUnitValidSkills(unitName);
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