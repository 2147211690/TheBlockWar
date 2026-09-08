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
    
    public bool IsSnapshot { get; private set; } = false;
    public Vector2I Size { get; private set; }
    public DefaultIdGenerator IdGenerator { get; private set; }
    public DateTime BaseTime { get; private set; }
    public IReadOnlyGameState? Owner => null;
    public ITileMapState TileMapState { get; private set; }
    public IUnitMapState UnitMapState { get; private set; }
    public IBuffGlobalState BuffGlobalState { get; private set; }
    public ITurnState TurnState { get; private set; }
    public IEnvState EnvState { get; private set; }
    IReadOnlyEventManagerState IReadOnlyGameState.EventManagerState => EventManagerState;
    IReadOnlyEnvState IReadOnlyGameState.EnvState => EnvState;
    public IEventManagerState EventManagerState { get; private set; }
    IReadOnlyTurnState IReadOnlyGameState.TurnState => TurnState;

    public TeamState TeamState { get; private set; }
    
    IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
    IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
    IReadOnlyBuffGlobalState IReadOnlyGameState.BuffGlobalState => BuffGlobalState;
    
    public IReadOnlyList<ISkill> GetUnitValidSkills(string unitName)
    {
        //TODO: 根据单位名称获取有效技能
        return unitName.GetUnitInfo().InitSkills.Select(a => a.SkillName.GetSkillInfo().Instance).ToList();
    }

    public IGameState GetSnapshot() => new GameStateSnapshot(this);

    public GameState(GameStateData data)
    {
        LoadGameData(data);
    }
    public GameState() : this(new (0,0), new int[0,0]){}
    public GameState(Vector2I mapSize, int[,] teamRelations, int currentTurn = -1)
    {
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
        EventManagerState = new EventManagerState(mapSize);
        EnvState = new EnvState();
    }

    public void LoadGameData(GameStateData data)
    {
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
        EventManagerState = new EventManagerState(data.EventManagerState);
        EnvState = new EnvState(data.EnvState);
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
            BuffGlobalState = BuffGlobalState.ToData(),
            TurnState = TurnState.ToData(),
            EventManagerState = EventManagerState.ToData(),
            EnvState = EnvState.ToData(),
        };
    }
    /// <summary>
    /// 增量游戏状态快照 - 写时复制（COW）实现
    /// 内存优化：仅存储变更，未变更数据保持对原状态的引用
    /// </summary>
    public sealed class GameStateSnapshot : IGameState
    {
        // 父状态引用 - 未修改数据的回退源
        public bool IsSnapshot { get; } = true;
        public Vector2I Size => Owner.Size;
        public DefaultIdGenerator IdGenerator => Owner.IdGenerator;
        public DateTime BaseTime => Owner.BaseTime;
        public IReadOnlyGameState Owner { get; }
        public ITileMapState TileMapState { get; }
        public IUnitMapState UnitMapState { get; }
        public IBuffGlobalState BuffGlobalState { get; }

        public IEnvState EnvState { get; }
        public TeamState TeamState => Owner.TeamState;
        public ITurnState TurnState { get; }
        public IEventManagerState EventManagerState { get; }
        IReadOnlyTurnState IReadOnlyGameState.TurnState => TurnState;
        IReadOnlyUnitMapState IReadOnlyGameState.UnitMapState => UnitMapState;
        IReadOnlyTileMapState IReadOnlyGameState.TileMapState => TileMapState;
        IReadOnlyBuffGlobalState IReadOnlyGameState.BuffGlobalState => BuffGlobalState;
        IReadOnlyEventManagerState IReadOnlyGameState.EventManagerState => EventManagerState;
        IReadOnlyEnvState IReadOnlyGameState.EnvState => EnvState;
        
        
        public GameStateSnapshot(IReadOnlyGameState owner)
        {
            // 只能是GameState或其快照
            Owner = owner;
            TurnState = owner.TurnState.GetSnapshot();
            TileMapState = owner.TileMapState.GetSnapshot();
            UnitMapState = owner.UnitMapState.GetSnapshot();
            BuffGlobalState = owner.BuffGlobalState.GetSnapshot();
            EventManagerState = owner.EventManagerState.GetSnapshot();
            EnvState = owner.EnvState.GetSnapshot();
        }
        public GameStateData ToData()
        {
            return new GameStateData
            {
                BaseTime = BaseTime,
                TileMapState = TileMapState.ToData(),
                UnitMapState = UnitMapState.ToData(),
                TeamState = TeamState.ToData(),
                Size = Size,
                BuffGlobalState = BuffGlobalState.ToData(),
                TurnState = TurnState.ToData(),
                EventManagerState = EventManagerState.ToData(),
                EnvState = EnvState.ToData(),
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
            EventManagerState.ClearIfSnapshot();
            EnvState.ClearIfSnapshot();
        }
        public IGameState GetSnapshot()
        {
            return new GameStateSnapshot(this);
        }
    }
}