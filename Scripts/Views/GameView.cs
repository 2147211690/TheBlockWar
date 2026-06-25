using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.EventViews;
using TheBlockWar.Scripts.Views.TileViews;
using TheBlockWar.Scripts.Views.UnitViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;

[GlobalClass, Icon("res://addons/at-icons/node2d/checkerboard.svg")]
public partial class GameView : Node2D, IGameView
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    [Export] public PackedScene UnitViewScene { get; private set; } = null!;
    [Export] public PackedScene TileViewScene { get; private set; } = null!;
    [Export] public float NormalGridHeight { get; set; } = 34;
    [Export] public GameViewStyle GameStyle { get; set; } = null!;
    
    
    [ExportGroup("Self")]
    [Export] public GameViewNodeContainer DrawContainer { get; private set; } = null!;
    [Export] public TilesView TilesView { get; set; } = null!;
    [Export] public UnitsView UnitsView { get; set; } = null!;
    [Export] public EventsView EventsView { get; set; } = null!;
    [Export] public Timer DrawEventTimer { get; set; } = null!;
    Node2D IGameView.DrawContainer => DrawContainer;
    public ViewPools Pools { get; private set; }
    public bool IsRunning => _runningQueue.IsRunning;
    public IReadOnlyGameState GameState => Scheduler.GameState;
    public IEventScheduler Scheduler { get; private set; }
    public GameViewTeamStyle CurrentStyle { get; private set; } = null!;
    public int PlayerTeam { get; set; }

    public TeamTypeEnum CurrentTeamType
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            CurrentStyle = GameStyle[value];
        }
    } = TeamTypeEnum.Enemy;

    public Vector2 NodeRight => -TilesView.LocalLeft;
    public Vector2 NodeLeft => TilesView.LocalLeft;
    public Vector2 NodeUp => TilesView.LocalUp;
    public Vector2 NodeDown => -TilesView.LocalUp;

    #region EventUpdate
    private TaskQueue _runningQueue = new();
    private Queue<ReadOnlySimObject> _eventQueue = new();
    private int _prevEventQueueCount = 0;
    private SimResultHandler _resultHandler;
    private bool _isEventDirty = true;
    private bool _isEventDrawn = false;
    #endregion
    
    #region Init

    public override void _Ready()
    {
        DrawContainer.GameView = this;
        Pools = new ViewPools(DrawContainer, UnitViewScene, TileViewScene);
        UnitsView.GameView = this;
        EventsView.GameView = this;
        TilesView.GameView = this;
        CurrentStyle = GameStyle[CurrentTeamType];
        DrawEventTimer.Timeout += CheckDrawEvent;
    }

    public void Init(IEventScheduler scheduler)
    {
        Scheduler = scheduler;
        Scheduler.SimulationUpdated += SchedulerOnSimulationUpdated;
        Scheduler.GameStateReload += SchedulerOnGameStateReload;
        _resultHandler = new SimResultHandler(HandleActionAsync);
        _runningQueue.IsRunningChanged += IsRunningChanged;

        void IsRunningChanged(object? sender, bool e)
        {
            Scheduler.IsRunning = e;
            Logger.Debug("运行状态改变{isRunning}", e);
            if (e) return;
            //运行结束
            
        }

        GDTask HandleActionAsync(ReadOnlySimObject arg1, StepResult arg2)
        {
            CurrentTeamType = GetTeamType(arg2.Team);
            return arg2.ViewRunAsync(this, arg1);
        }
    }

    private void SchedulerOnGameStateReload(object? sender, IReadOnlyGameState e)
    {
        ClearEventViews();
        _runningQueue.Clear();
        TilesView.ClearAllTile();
        UnitsView.ClearAllUnit();
        var size = e.Size;
        for (int i = 0; i < size.X; i++)
        {
            for (int j = 0; j < size.Y; j++)
            {
                var tile = e.TileMapState.GetTile(new Vector2I(i, j));
                TilesView.SetTileView(new(i, j), tile.Name);
                foreach (var region in tile.Regions)
                {
                    //TODO: 绘制区域
                }
            }
        }
        foreach (var unit in e.UnitMapState.GetAllUnits())
        {
            var view = UnitsView.CreateUnitView(unit.Coords, unit)!;
        }
        _prevEventQueueCount = 0;
        _isEventDirty = true;
        _isEventDrawn = false;
    }

    private void SchedulerOnSimulationUpdated(object? sender, SchedulerResult e)
    {
        int eventCount = e.Sims.Count - e.RunningCount;
        if (eventCount != 0 || _prevEventQueueCount != 0) 
            _isEventDirty = true;

        if (e.RunningCount > 0)
        {
            ClearEventViews();
            var running = e.Sims.GetRange(0, e.RunningCount);
            _runningQueue.AddTask(() => _resultHandler.HandleAsync(running));
        }

        _eventQueue.Clear();
        for (int i = e.RunningCount; i < e.Sims.Count; i++)
        {
            _eventQueue.Enqueue(e.Sims[i]);
        }
    }

    #endregion
    
    private void CheckDrawEvent()
    {
        if (_isEventDirty && !_runningQueue.IsRunning)
        {
            _isEventDirty = false;
            ClearEventViews();
            DrawEventViews();
        }
    }
    private void DrawEventViews()
    {
        if (_isEventDrawn) return;
        _isEventDrawn = true;
        foreach (var e in _eventQueue)
        {
            foreach (var s in e.Steps)
            {
                CurrentTeamType = GetTeamType(s.Team);
                s.ViewDraw(this, e);
            }
        }
        _prevEventQueueCount = _eventQueue.Count;
        _eventQueue.Clear();
    }

    private void ClearEventViews()
    {
        if (!_isEventDrawn) return;
        _isEventDrawn = false;
        EventsView.EventClear();
    }
    
    public TeamTypeEnum GetTeamType(int team)
    {
        return GameState.TeamState.GetTeamTypeRelativeToPlayer(PlayerTeam, team);
    }

    public Vector3 CoordsToGamePosition(Vector2 coords, float heightOffset = 0, float? height = null)
    {
        return TilesView.CoordsToGamePosition(coords, heightOffset, height);
    }

    public Vector2 GamePositionToNodePosition(in Vector3 gamePosition)
    {
        return TilesView.GamePositionToNodePosition(gamePosition);
    }

    public Vector2I NodePositionToCoords(Vector2 nodePosition)
    {
        return TilesView.NodePositionToCoords(nodePosition);
    }

    public UnitView? CreateUnitView(Vector2I coords, IReadOnlyUnitState unitState)
    {
        return UnitsView.CreateUnitView(coords, unitState);
    }

    public UnitView? GetUnitView(long id)
    {
        return UnitsView.GetUnitView(id);
    }

    public UnitView? GetUnitView(Vector2I coords)
    {
        return UnitsView.GetUnitView(coords);
    }

    public void FreeUnitView(long id)
    {
        UnitsView.FreeUnitView(id);
    }

    public void SetUnitRealCoords(long id, Vector2I coords)
    {
        UnitsView.SetUnitRealCoords(id, coords);
    }

    public TileView? SetTileView(Vector2I coords, string tileName)
    {
        return TilesView.SetTileView(coords, tileName);
    }

    public TileView? GetTileView(Vector2I coords)
    {
        return TilesView.GetTileView(coords);
    }

    public float GetTileHeight(Vector2I coords)
    {
        return TilesView.GetTileHeight(coords);
    }

    public TileInfo GetTileInfo(Vector2I coords)
    {
        return TilesView.GetTileInfo(coords);
    }


    public UnitView? EventUnit(long id)
    {
        return EventsView.EventUnit(id);
    }

    public ViewSprite2D EventSprite()
    {
        return EventsView.EventSprite();
    }

    public void EventTopDraw(Action<ViewNode2D> action, int zOffset = 0)
    {
        EventsView.EventTopDraw(action, zOffset);
    }

    public ViewNode2D EventDraw(Action<ViewNode2D> action)
    {
        return EventsView.EventDraw(action);
    }

    public void EventHealth(Vector2I coords, float originHealthChange, float validHealthChange, bool hasTarget, bool isFocused)
    { 
        EventsView.BlockDraw.AddHealthChange(coords, originHealthChange, validHealthChange, hasTarget, isFocused);
    }

    public void EventBuff(Vector2I coords, string buffName, int countChange, bool isFocused)
    {
        EventsView.BlockDraw.AddBuffChange(coords, buffName, countChange > 0, isFocused);
    }

    public void EventRegion(Vector2I coords, string regionName, bool isAdd, bool isFocused)
    {
        EventsView.BlockDraw.AddRegionChange(coords, regionName, isAdd, isFocused);
    }

    public ViewNode2D? CreateEffect(string effectName)
    {
        var effectScene = CurrentStyle.EffectTable.EffectScenes.GetValueOrDefault(effectName);
        if (effectScene == null) return null;
        var effect = effectScene.Instantiate<ViewNode2D>();
        DrawContainer.AddChild(effect);
        return effect;
    }

    public void ShowRegion(IEnumerable<Vector2I> coords, int team, RegionShowTypeEnum showTypeEnum)
    {
        TilesView.ShowRegion(coords, team, showTypeEnum);
    }

    public void ClearRegion()
    {
        TilesView.ClearRegion();
    }
}