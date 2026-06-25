using System.Linq;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Tools;
using Yitter.IdGenerator;

namespace TheBlockWar.Scripts.Games.GameStates;

public class GameState : IGameState
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    
    public IDictionary<long, SchedulerEvent> Events => _events;
    public bool IsSnapshot { get; private set; } = false;
    IReadOnlyDictionary<long, SchedulerEvent> IReadOnlyGameState.Events => _events;
    public Vector2I Size { get; private set; }
    public DefaultIdGenerator IdGenerator { get; private set; }
    public DateTime BaseTime { get; private set; }
    public IReadOnlyGameState? Owner => null;
    public ITileMapState TileMapState { get; private set; }
    public IUnitMapState UnitMapState { get; private set; }
    public IBuffGlobalState BuffGlobalState { get; private set; }
    public ITurnState TurnState { get; private set; }
    IReadOnlyTurnState IReadOnlyGameState.TurnState => TurnState;

    public TeamState TeamState { get; private set; }
    
    IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
    IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
    IReadOnlyBuffGlobalState IReadOnlyGameState.BuffGlobalState => BuffGlobalState;
    
    private Dictionary<long, SchedulerEvent> _events;

    public IReadOnlyList<ISkill> GetUnitValidSkills(string unitName)
    {
        //TODO: 根据单位名称获取有效技能
        return unitName.GetUnitInfo().Skills.Select(a => a.GetSkillInfo().Instance).ToList();
    }

    public IGameState GetSnapshot() => new GameStateSnapshot(this);

    public GameState(GameStateData data)
    {
        LoadGameData(data);
    }
    public GameState() : this(new (0,0), new int[0,0]){}
    public GameState(Vector2I mapSize, int[,] teamRelations, int currentTurn = -1)
    {
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
        BuffGlobalState = new BuffGlobalState();
        TeamState = new TeamState(teamRelations);
        TurnState = new TurnState(currentTurn);
    }

    public void LoadGameData(GameStateData data)
    {
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
        BuffGlobalState = new BuffGlobalState(data.BuffGlobalState);
        TurnState = new TurnState(data.TurnState);
    }

    public void ClearIfSnapshot()
    {
    }
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
            BuffGlobalState = BuffGlobalState.ToData(),
            TurnState = TurnState.ToData(),
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
        public bool IsSnapshot { get; } = true;
        IDictionary<long, SchedulerEvent>? IGameState.Events => null;

        public Vector2I Size => Owner.Size;
        public DefaultIdGenerator IdGenerator => Owner.IdGenerator;
        public DateTime BaseTime => Owner.BaseTime;
        public IReadOnlyGameState Owner { get; }
        public ITileMapState TileMapState { get; }
        public IUnitMapState UnitMapState { get; }
        public IBuffGlobalState BuffGlobalState { get; }
        public TeamState TeamState => Owner.TeamState;
        public ITurnState TurnState { get; }
        IReadOnlyTurnState IReadOnlyGameState.TurnState => TurnState;
        IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
        IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
        IReadOnlyBuffGlobalState IReadOnlyGameState.BuffGlobalState => BuffGlobalState;
        public GameStateSnapshot(IReadOnlyGameState owner)
        {
            // 只能是GameState或其快照
            Owner = owner;
            TurnState = owner.TurnState.GetSnapshot();
            TileMapState = owner.TileMapState.GetSnapshot();
            UnitMapState = owner.UnitMapState.GetSnapshot();
            BuffGlobalState = owner.BuffGlobalState.GetSnapshot();
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
                BuffGlobalState = BuffGlobalState.ToData(),
                TurnState = TurnState.ToData(),
            };
        }

        public IReadOnlyList<ISkill> GetUnitValidSkills(string unitName) => Owner.GetUnitValidSkills(unitName);
        public void LoadGameData(GameStateData data) => Logger.Warn("快照不应该加载数据");

        public void ClearIfSnapshot()
        {
            TileMapState.ClearIfSnapshot();
            UnitMapState.ClearIfSnapshot();
            BuffGlobalState.ClearIfSnapshot();
            TurnState.ClearIfSnapshot();
        }
        public IGameState GetSnapshot()
        {
            return new GameStateSnapshot(this);
        }
    }
}