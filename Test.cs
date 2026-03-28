using ImGuiNET;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Modes;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;
using TheBlockWar.View.Uis;
using Yitter.IdGenerator;

namespace TheBlockWar;

public partial class Test : Node2D
{
    IGameState? _currentGameState;
    IGameState? _snapshot;
    IGameState? _gameState;
    SimulationResult? _simulationResult;
    uint _nextUnitId = 1;
    int x = 0, y = 0;
    int unitId = 1, newX = 0, newY = 0;
    private int deleteId = 1;
    int getById = 1;
    int getByX = 0, getByY = 0;
    bool _showSnapshot = false;
    private int damageTarget;
    private int damageValue;
    private GameScheduler _gameScheduler = new();
    private SchedulerEvent _event0;
    [Export] private GameView _view;
    [Export] private CombatUi _combatUi;
    private SchedulerEvent _event2;
    private SchedulerEvent _move;
    private SchedulerEvent _damage;
    private SchedulerEvent _follow;
    private StatePlayer _player;
    private PvpMode _pvpMode;
    private TurnManager _turnManager;
    private bool _checkNextTurn;
    

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
        
        TaskTracker.ShowTrackerWindow();
        
        _currentGameState = new GameState(new Vector2I(10, 10),
            new Dictionary<string, ISkill[]>() 
            {
                {"Unit_0", [Skills.Skill1]},
            },
            new int[,] 
            {
                {0,1,3},
                {1,0,3},
                {3,3,0},
            }
            );
        _gameState = _currentGameState;
        _gameScheduler.InitScheduler(_currentGameState, 0);
        _snapshot = null;

        _player = new StatePlayer();
        _player.GameScheduler = _gameScheduler;
        AddChild(_player);
        
        _view.Bind(_gameScheduler);
        _view.Player = _player;
        
        _pvpMode = new PvpMode([_player], _player);
        _turnManager = new TurnManager(_gameScheduler, _view, _pvpMode);
        _player.TurnManager = _turnManager;
        _combatUi.Bind(_gameScheduler, _player);
        _turnManager.Start().Forget();
    }

    public override void _Process(double delta)
    {
        ImGui.Begin("游戏状态");
        
        if (_currentGameState is null)
        {
            ImGui.Text("游戏状态未初始化");
        }
        else
        {
            // 切换显示快照还是本体
            ImGui.Checkbox("显示快照", ref _showSnapshot);
            
            // 显示游戏状态信息
            _currentGameState = _showSnapshot && _snapshot != null ? _snapshot : _gameState;
            
            // 添加 ToInfoString 按钮
            if (ImGui.Button("显示详细信息 (ToInfoString)"))
            {
                GD.Print(_currentGameState.ToInfoString());
            }
            
            // 测试添加单位
            ImGui.Separator();
            ImGui.Text("添加单位");
            
            ImGui.InputInt("X", ref x);
            ImGui.InputInt("Y", ref y);
            
            if (ImGui.Button("添加单位"))
            {
                var coords = new Vector2I(x, y);
                var unit = UnitFactory.CreateUnit("Unit_0", 0);
                _currentGameState.UnitMapState.AddUnit(unit);
            }
            
            // 测试修改单位
            ImGui.Separator();
            ImGui.Text("修改单位");
            
            ImGui.InputInt("单位ID", ref unitId);
            ImGui.InputInt("新X", ref newX);
            ImGui.InputInt("新Y", ref newY);
            
            if (ImGui.Button("修改单位"))
            {
                var unit = _currentGameState.UnitMapState.GetUnit((uint)unitId);
                if (unit is not null)
                {
                    var newCoords = new Vector2I(newX, newY);
                    _currentGameState.UnitMapState.ModifyUnit((uint)unitId, u => u.Coords = newCoords);
                }
            }
            
            // 测试删除单位
            ImGui.Separator();
            ImGui.Text("删除单位");
            
            ImGui.InputInt("删除ID", ref deleteId);
            
            if (ImGui.Button("删除单位"))
            {
                _currentGameState.UnitMapState.RemoveUnit((uint)deleteId);
            }
            
            // 测试获取单位
            ImGui.Separator();
            ImGui.Text("获取单位");
            
            ImGui.InputInt("按ID获取", ref getById);
            if (ImGui.Button("获取单位(按ID)"))
            {
                var unit = _currentGameState.UnitMapState.GetUnit((uint)getById);
                if (unit is not null)
                {
                    GD.Print($"找到单位: ID={unit.Id}, 坐标=({unit.Coords.X}, {unit.Coords.Y})");
                }
                else
                {
                    GD.Print("未找到单位");
                }
            }
            
            ImGui.InputInt("按坐标X", ref getByX);
            ImGui.InputInt("按坐标Y", ref getByY);
            if (ImGui.Button("获取单位(按坐标)"))
            {
                var unit = _currentGameState.UnitMapState.GetUnit(new Vector2I(getByX, getByY));
                if (unit is not null)
                {
                    GD.Print($"找到单位: ID={unit.Id}, 坐标=({unit.Coords.X}, {unit.Coords.Y})");
                }
                else
                {
                    GD.Print("该坐标无单位");
                }
            }
            
            // 测试快照功能
            ImGui.Separator();
            ImGui.Text("快照功能测试");
            
            if (ImGui.Button("创建快照"))
            {
                _snapshot = _currentGameState.GetSnapshot();
                ImGui.Text("快照创建成功!");
            }
            
            if (_snapshot != null)
            {
                ImGui.Text("快照状态:");
                ImGui.Text(_snapshot.ToString());
            }
        }
        
        ImGui.End();
        

        ImGui.Begin("游戏调度器");
        if (ImGui.Button("输出调度器信息"))
        {
            GD.Print("调度结果");
            foreach (var e in _gameScheduler.SimulationResults)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e.Steps.Count}");
            }
            GD.Print("当前运行层");
            foreach (var e in _gameScheduler.RunningEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前关注层");
            foreach (var e in _gameScheduler.FocusedEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前等待层");
            foreach (var e in _gameScheduler.WaitingEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前技能层");
            foreach (var e in _gameScheduler.SkillEvents.Values)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
        }
        if (ImGui.Button("添加调度事件"))
        {
            _event0 = new ActionSchedulerEvent()
            {
                ApplyAction = e =>
                {
                    e.State.ApplyDamage(new(3, 3),
                        10, e.Result);
                }
            };
            _gameScheduler.AddEvent(_event0);
        }

        if (ImGui.Button("触发调度事件"))
        {
            _event0.Layer = SimulationLayer.Running;
        }
        if (ImGui.Button("测试放置压力"))
        {

            _gameScheduler.AddEvent(new ActionSchedulerEvent()
            {
                ApplyAction = e =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            e.State.ApplyPlaceUnit(new(i, j), "Unit_0", 1, null, e.Result);
                        }
                    }
                }
            }, SimulationLayer.Running);
        }
        if (ImGui.Button("测试放置"))
        {

            _gameScheduler.AddEvent(new ActionSchedulerEvent( e =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            var empty = false || i is > 2 and < 7 && j is > 2 and < 7;
                            e.State.ApplyPlaceTile(new(i, j), new TileState()
                                {
                                    Tag = empty ? TileTag.Empty : TileTag.Ground
                                },
                                e.Result);
                        }
                    }
                    e.State.ApplyPlaceUnit(new(2, 2), "Unit_0", 0, null, e.Result);
                }), SimulationLayer.Running);
        }
        if (ImGui.Button("测试放置2"))
        {

            _event2 = _gameScheduler.AddEvent(new ActionSchedulerEvent()
            {
                ApplyAction = e =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            e.State.ApplyPlaceUnit(new(i, j), "Unit_0", 1, null, e.Result);
                        }
                    }
                }
            });
        }

        if (ImGui.Button("使用事件2"))
        {
            _event2.Layer = SimulationLayer.Running;
        }
        
        if (ImGui.Button("测试移动2,2 -> 6,3"))
        {

            _move = _gameScheduler.AddEvent(new ActionSchedulerEvent(e =>
            {
                 e.State.ApplyMoveUnitPath(new(2,2), 
                     [new(0,0), new(0,1), new(0,2),new(1,2),new(2,2)],
                     e.Result);
            }));
        }
        if (ImGui.Button("添加十字单位"))
        {
            _gameScheduler.AddEvent(new ActionSchedulerEvent(e =>
            {
                e.State.ApplyPlaceUnit(new(3,2), "Unit_0", 0, null, e.Result);
                e.State.ApplyPlaceUnit(new(2,3), "Unit_0", 0, null, e.Result);
                e.State.ApplyPlaceUnit(new(1,2), "Unit_0", 0, null, e.Result);
                e.State.ApplyPlaceUnit(new(2,1), "Unit_0", 0, null, e.Result);
            }), SimulationLayer.Running);
        }

        if (ImGui.Button("使用移动"))
        {
            _move.Layer = SimulationLayer.Running;
        }

        if (ImGui.Button("改变移动的关注性"))
        {
            _move.Layer = _move.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }
        
        if (ImGui.Button("测试伤害"))
        {

            _damage = _gameScheduler.AddEvent(new ActionSchedulerEvent(e =>
            {
                 e.State.ApplyDamage(new(2,2), 50, e.Result);
            }));
        }

        if (ImGui.Button("使用伤害"))
        {
            _damage.Layer = SimulationLayer.Running;
        }
        if (ImGui.Button("改变伤害的关注性"))
        {
            _damage.Layer = _damage.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }

        if (ImGui.Button("添加跟随技能"))
        {
            _follow = _gameScheduler.AddEvent(new SkillSchedulerEvent(1, Skills.Skill1, 0));
        }
        if (ImGui.Button("改变跟随的关注性"))
        {
            _follow.Layer = _follow.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }
        
        ImGui.End();

        ImGui.Begin("回合");
        if (ImGui.Button("完成回合"))
        {
            _player.IsTurn = false;
        }

        if (ImGui.Checkbox("关注结束回合", ref _checkNextTurn))
        {
            _turnManager.FocusTurnEvent(_checkNextTurn ? _turnManager.CurrentTurn + 1 : -1);
        }

        if (ImGui.Button("添加到下一回合事件"))
        {
            _turnManager.AddTurnEvent(new ActionSchedulerEvent(e =>
            {
                e.State.ApplyDamage(new(2,2), 50, e.Result);
            })
            {
                Turn = _turnManager.CurrentTurn + 1
            });
        }
        ImGui.End();
    }
}