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
    private SchedulerEvent _event0;
    [Export] private GameView _view;
    [Export] private CombatUi _combatUi;
    private SchedulerEvent _event2;
    private SchedulerEvent _move;
    private SchedulerEvent _damage;
    private SchedulerEvent _follow;
    private StatePlayer _player;
    private PvpTurnStrategy _pvpMode;
    private TurnManager _turnManager;
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
        _player = StatePlayer.Create(_gameState.IdGenerator.NewLong(),  _view, _eventScheduler, playerOperationSynchronizer);
        AddChild(_player);
        
        _view.Init(_eventScheduler);
        _view.PlayerTeam = _player.Team;
        
        _pvpMode = new PvpTurnStrategy(
            playerOperationSynchronizer, 
            [
                new EasyAiPlayer(1, 1, _eventScheduler, playerOperationSynchronizer), 
                _player,
            ],
            new FlatMapBuilder(TileNames.Soil0)
            );
        _turnManager = new TurnManager(_eventScheduler, _pvpMode);
        _player.TurnManager = _turnManager;
        _combatUi.Bind(_eventScheduler, _view, _player);
        _turnManager.Start().Forget();

        //TestPlace();
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
                var unit = UnitFactory.CreateUnit(UnitNames.Unit0, 0, _gameState.IdGenerator.NewLong(), coords);
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
            GD.Print("当前运行层");
            foreach (var e in _eventScheduler.RunningEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前关注层");
            foreach (var e in _eventScheduler.FocusedEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前等待层");
            foreach (var e in _eventScheduler.WaitingEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前技能层");
            foreach (var e in _eventScheduler.SkillEvents.Values)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
            GD.Print("当前未激活事件");
            foreach (var e in _eventScheduler.DisabledEvents)
            {
                GD.Print($"{e.GetType()}: {e.Layer} {e}");
            }
        }

        if (ImGui.Button("触发调度事件"))
        {
            _event0.Layer = SimulationLayer.Running;
        }
        if (ImGui.Button("测试放置压力"))
        {

            _eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (e, obj) =>
                {
                    obj.BeginSteps(0, 0, StepsRunningMode.QueueWaitAll);
                    for (int i = 0; i < 10; i++)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            obj.ApplyPlaceUnit(new(i, j), UnitNames.Unit0, 1);
                        }
                    }
                    obj.EndSteps();
                },
                Layer = SimulationLayer.Running,
            });
        }
        if (ImGui.Button("测试放置"))
        {
            TestPlace();
        }
        if (ImGui.Button("测试掉落"))
        {

            _eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (e, obj) =>
                {
                    for (int i = 0; i < 10; i++)
                    {
                        obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
                        for (int j = 0; j < 10; j++)
                        {
                            obj.ApplyDropTile(new (i, j));
                        }
                        obj.EndSteps();
                    }
                    obj.ApplyPlaceUnit(new(5, 5), UnitNames.Unit0, 0);
                },
                Layer = SimulationLayer.Running,
            });
        }
        ImGui.InputText("BuffName", ref _buffName, 100);
        if (ImGui.Button("添加力Buff"))
        {
            _eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (e, obj) =>
                {
                    foreach (var unit in obj.State.UnitMapState.GetAllUnits())
                    {
                        obj.ApplyModifyUnitBuff(unit.Coords, _buffName, BuffOperation.AddBuff, 1, false);
                    }
                },
                Layer = SimulationLayer.Running,
            });
        }
        if (ImGui.Button("移除Buff"))
        {
            _eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (e, obj) =>
                {
                    foreach (var unit in obj.State.UnitMapState.GetAllUnits())
                    {
                        obj.ApplyModifyUnitBuff(unit.Coords, _buffName, BuffOperation.RemoveBuff, 1, false);
                    }
                },
                Layer = SimulationLayer.Running,
            });
        }
        
        if (ImGui.Button("添加十字单位"))
        {
            _eventScheduler.CreateEvent(id => new ActionEvent(id)
            {
                ActionCore = (e, obj) =>
                {
                    obj.ApplyPlaceUnit(new(3,2), UnitNames.Unit0, 0);
                    obj.ApplyPlaceUnit(new(2,3), UnitNames.Unit0, 0);
                    obj.ApplyPlaceUnit(new(1,2), UnitNames.Unit0, 0);
                    obj.ApplyPlaceUnit(new(2,1), UnitNames.Unit0, 0);
                },
                Layer = SimulationLayer.Running,
            });
        }

        if (ImGui.Button("添加一个测试步骤事件"))
        {
            _eventScheduler.CreateEvent(id => new StepsEvent(id)
            {
                Steps = [
                new ApplyDamageStep(new (2,2), 10),
                ],
                Layer = SimulationLayer.Waiting,
            });
        }

        if (ImGui.Button("使用移动"))
        {
            _move.Layer = SimulationLayer.Running;
        }

        if (ImGui.Button("改变移动的关注性"))
        {
            _move.Layer = _move.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }

        if (ImGui.Button("使用伤害"))
        {
            _damage.Layer = SimulationLayer.Running;
        }
        if (ImGui.Button("改变伤害的关注性"))
        {
            _damage.Layer = _damage.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
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
            _turnManager.FocusNextTurn(_checkNextTurn ? _turnManager.CurrentTurn + 1 : -1);
        }
        
        ImGui.End();
        
        
        ImGui.Begin("Json");

        if (ImGui.Button("序列化TileState"))
        {
            var json = JsonSerializer.Serialize(_gameState.TileMapState.ToData(), _jsonSerializerOptions);
            GD.Print(json);
        }
        if (ImGui.Button("序列化UnitState"))
        {
            var json = JsonSerializer.Serialize(_gameState.UnitMapState.ToData(), _jsonSerializerOptions);
            GD.Print(json);
        }
        if (ImGui.Button("序列化TeamState"))
        {
            var json = JsonSerializer.Serialize(_gameState.TeamState.ToData(), _jsonSerializerOptions);
            GD.Print(json);
        }
        if (ImGui.Button("序列化GameState"))
        {
            var json = JsonSerializer.Serialize(_gameState.ToData(), _jsonSerializerOptions);
            GD.Print(json);
        }
        if (ImGui.Button("测试序列化Apply"))
        {
            var apply = new ApplyStepsStep(
                [
                    new ApplyPlaceUnitStep(new(1,1), UnitNames.Unit0, 0),
                    
                ], 
                0, 0, StepsRunningMode.QueueWaitAll);
            var json = JsonSerializer.Serialize(apply, _jsonSerializerOptions);
            GD.Print(json);
        }
        if (ImGui.Button("保存GameState到数据"))
        {
            var json = JsonSerializer.Serialize(_gameState.ToData(), _jsonSerializerOptions);
            GD.Print(json);
            using var file = FileAccess.Open("res://testData.json", FileAccess.ModeFlags.Write);
            file.StoreString(json);
        }
        if (ImGui.Button("从数据加载GameState"))
        {
            var data = LoadJsonFromRes<GameStateData>("res://testData.json");
            SaveManager.LoadCurrentGameState(data, _gameState, _eventScheduler);
        }
        ImGui.End();

        UnitEditor();
    }

    private void TestPlace()
    {
        string[] names = [TileNames.Soil0, TileNames.水块, TileNames.熔岩块, TileNames.燃烧块];
        _eventScheduler.CreateEvent(id => new ActionEvent(id)
        {
            ActionCore = (e, obj) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    obj.BeginSteps(0, 0, StepsRunningMode.ParallelWaitAll);
                        
                    for (int j = 0; j < 10; j++)
                    {
                        var empty = false || i is > 2 and < 7 && j is > 2 and < 7;
                        obj.ApplyPlaceTile(new(i, j), empty ? TileNames.水块 : 
                            (names[Random.Shared.Next(names.Length)]));
                    }
                    obj.EndSteps();
                }
                obj.ApplyPlaceUnit(new(5, 5), UnitNames.Unit0, 0);
                obj.ApplyPlaceUnit(new(3, 3), UnitNames.Unit1, 1);
                obj.ApplyPlaceUnit(new(5, 5), UnitNames.Unit1, 1);
                obj.ApplyPlaceUnit(new(4, 2), UnitNames.Unit2, 1);
                obj.ApplyPlaceUnit(new(1, 4), UnitNames.Unit2, 1);
                obj.ApplyPlaceUnit(new(6, 1), UnitNames.Unit0, 1);
                obj.ApplyPlaceUnit(new(1, 5), UnitNames.Unit1, 1);
            },
            Layer = SimulationLayer.Running,
        });
    }

    private T LoadJsonFromRes<T>(string path) where T : class
    {
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"无法打开文件: {path}");
            return null;
        }
        string json = file.GetAsText();
        return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions);
    }

    private void UnitEditor()
    {
        ImGui.Begin("单位编辑器");

        if (_currentGameState is null)
        {
            ImGui.Text("游戏状态未初始化");
            ImGui.End();
            return;
        }

        ImGui.Separator();
        ImGui.Text("查看/编辑单位");
        ImGui.InputInt("单位ID", ref _editUnitIdInt);

        var unit = _currentGameState.UnitMapState.GetUnit(_editUnitIdInt);
        if (!unit.IsNull())
        {
            ImGui.Text($"名称: {unit.Name}");
            ImGui.Text($"队伍: {unit.Team}");
            ImGui.Text($"坐标: ({unit.Coords.X}, {unit.Coords.Y})");
            ImGui.Text($"生命值: {unit.Health}/{unit.MaxHealth}");
            ImGui.Text($"护盾值: {unit.Shield}/{unit.MaxShield}");
            ImGui.Text($"是否死亡: {unit.IsDead}");
            ImGui.Text($"是否飞行: {unit.IsFlying}");

            if (unit.Buffs.Count > 0)
            {
                ImGui.Text("Buff列表:");
                foreach (var buff in unit.Buffs)
                {
                    ImGui.Text($"  {buff.BuffName} x{buff.Count}");
                }
            }
            else
            {
                ImGui.Text("Buff列表: 无");
            }

            ImGui.Separator();
            ImGui.Text("编辑属性");

            int coordX = unit.Coords.X;
            int coordY = unit.Coords.Y;
            ImGui.InputInt("新坐标X", ref coordX);
            ImGui.InputInt("新坐标Y", ref coordY);
            if (ImGui.Button("应用坐标"))
            {
                _currentGameState.UnitMapState.ModifyUnit(_editUnitIdInt, u =>
                {
                    u.Coords = new Vector2I(coordX, coordY);
                });
            }

            float health = unit.Health;
            ImGui.SliderFloat("生命值", ref health, 0, unit.MaxHealth);
            if (ImGui.Button("应用生命值"))
            {
                _currentGameState.UnitMapState.ModifyUnit(_editUnitIdInt, u =>
                {
                    u.Health = health;
                });
            }

            float shield = unit.Shield;
            ImGui.SliderFloat("护盾值", ref shield, 0, unit.MaxShield);
            if (ImGui.Button("应用护盾值"))
            {
                _currentGameState.UnitMapState.ModifyUnit(_editUnitIdInt, u =>
                {
                    u.Shield = shield;
                });
            }

            if (ImGui.Button($"{(unit.IsDead ? "复活" : "杀死")}单位"))
            {
                _currentGameState.UnitMapState.ModifyUnit(_editUnitIdInt, u =>
                {
                    u.IsDead = !u.IsDead;
                });
            }

            if (ImGui.Button($"切换飞行状态 (当前: {(unit.IsFlying ? "飞行" : "地面")})"))
            {
                _currentGameState.UnitMapState.ModifyUnit(_editUnitIdInt, u =>
                {
                    u.IsFlying = !u.IsFlying;
                });
            }

            ImGui.Separator();
            if (ImGui.Button("删除此单位"))
            {
                _currentGameState.UnitMapState.RemoveUnit(_editUnitIdInt);
            }
        }
        else
        {
            ImGui.Text("未找到该单位");
        }

        ImGui.Separator();
        ImGui.Text("生成单位");
        ImGui.InputText("单位名称", ref _spawnUnitName, 100);
        ImGui.InputInt("生成X", ref _spawnUnitX);
        ImGui.InputInt("生成Y", ref _spawnUnitY);
        ImGui.InputInt("队伍", ref _spawnUnitTeam);

        if (ImGui.Button("生成单位"))
        {
            var coords = new Vector2I(_spawnUnitX, _spawnUnitY);
            _eventScheduler.RunAction(obj => obj.ApplyPlaceUnit(coords, _spawnUnitName, _spawnUnitTeam));
        }

        ImGui.End();
    }
}