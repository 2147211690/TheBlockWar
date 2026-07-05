using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using ImGuiNET;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.GameStates.Datas;
using TheBlockWar.Scripts.Games.Modes;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using Yitter.IdGenerator;
using CombatUi = TheBlockWar.Scripts.Views.Uis.CombatUi;

namespace TheBlockWar;

public partial class Test : Node2D
{
    IGameState? _currentGameState;
    IGameState? _snapshot;
    IGameState? _gameState;
    SimContext? _simulationResult;
    uint _nextUnitId = 1;
    int x = 0, y = 0;
    int unitId = 1, newX = 0, newY = 0;
    private int deleteId = 1;
    int getById = 1;
    int getByX = 0, getByY = 0;
    bool _showSnapshot = false;
    private int damageTarget;
    private int damageValue;
    private EventScheduler _eventScheduler;

    private StatePlayer _player;
    private PvpTurnStrategy _pvpMode;
    private ITurnManager _turnManager;
    private bool _checkNextTurn;
    private string _buffName = string.Empty;
    private int _editUnitIdInt;
    private string _spawnUnitName = UnitNames.Unit0;
    private int _spawnUnitX;
    private int _spawnUnitY;
    private int _spawnUnitTeam;
    private JsonSerializerOptions _jsonSerializerOptions = new()
    {
        WriteIndented = true,
        IncludeFields = true,
    };
    public override void _Ready()
    {

        //配置Yitter.IdGenerator.IdHelper
        var options = new IdGeneratorOptions
        {
            // 核心：WorkerId（机器/进程唯一标识，0~63，分布式必须唯一）
            WorkerId = 1, //TODO:联机需要根据机器ID设置
                
            // 可选：WorkerId占用位数（默认6，最大63）
            // WorkerIdBitLength = 6,
                
            // 可选：序列号位数（默认6，每毫秒最多 2^6-1=63 个ID）
            // SeqBitLength = 6,
                
            // 可选：基准时间（默认1970-01-01，建议设为项目启动时间）
            // BaseTime = new DateTime(2025, 1, 1),
        };
        YitIdHelper.SetIdGenerator(options);
        
        
        _currentGameState = 
        _gameState = _currentGameState;
        _eventScheduler = new EventScheduler(_currentGameState, 0);
        _snapshot = null;

        var playerOperationSynchronizer = new OnlinePlayerOperationSynchronizer();
        AddChild(playerOperationSynchronizer);
        AddChild(_player);
        

        //TestPlace();
    }



    public override void _Process(double delta)
    {
    }
}