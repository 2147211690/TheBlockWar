using Godot;
using ImGuiNET;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Player;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

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
    private SchedulerEvnet _event0;
    [Export] private GameView _view;
    private SchedulerEvnet _event2;
    private SchedulerEvnet _move;
    private SchedulerEvnet _damage;
    private SchedulerEvnet _follow;
    private StatePlayer _player;
    public override void _Ready()
    {
        _currentGameState = new GameState(new Vector2I(10, 10));
        _gameState = _currentGameState;
        _gameScheduler.SetGameState(_currentGameState);
        _snapshot = null;

        _player = new StatePlayer();
        _player.GameScheduler = _gameScheduler;
        AddChild(_player);
        
        _view.Bind(_gameScheduler);
        _view.Player = _player;
        
    }

    public override void _Process(double delta)
    {
        ImGui.Begin("游戏状态");
        
        if (_currentGameState == null)
        {
            ImGui.Text("游戏状态未初始化");
            if (ImGui.Button("初始化游戏状态"))
            {
                _currentGameState = new GameState(new Vector2I(10, 10));
                _gameState = _currentGameState;
                _gameScheduler.SetGameState(_currentGameState);
                _snapshot = null;
                _view.Bind(_gameScheduler);
            }
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
                var unit = UnitFactory.CreateUnit("Unit_0");
                _currentGameState.AddUnit(unit);
            }
            
            // 测试修改单位
            ImGui.Separator();
            ImGui.Text("修改单位");
            
            ImGui.InputInt("单位ID", ref unitId);
            ImGui.InputInt("新X", ref newX);
            ImGui.InputInt("新Y", ref newY);
            
            if (ImGui.Button("修改单位"))
            {
                var unit = _currentGameState.GetUnit((uint)unitId);
                if (unit is not null)
                {
                    var newCoords = new Vector2I(newX, newY);
                    _currentGameState.ModifyUnit((uint)unitId, u => u.Coords = newCoords);
                }
            }
            
            // 测试删除单位
            ImGui.Separator();
            ImGui.Text("删除单位");
            
            ImGui.InputInt("删除ID", ref deleteId);
            
            if (ImGui.Button("删除单位"))
            {
                _currentGameState.RemoveUnit((uint)deleteId);
            }
            
            // 测试获取单位
            ImGui.Separator();
            ImGui.Text("获取单位");
            
            ImGui.InputInt("按ID获取", ref getById);
            if (ImGui.Button("获取单位(按ID)"))
            {
                var unit = _currentGameState.GetUnit((uint)getById);
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
                var unit = _currentGameState.GetUnit(new Vector2I(getByX, getByY));
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
            foreach (var e in _gameScheduler.RunningSimulations)
            {
                GD.Print($"{e.GetType()}: {e.Layer}");
            }
            GD.Print("当前关注层");
            foreach (var e in _gameScheduler.FocusedSimulations)
            {
                GD.Print($"{e.GetType()}: {e.Layer}");
            }
            GD.Print("当前等待层");
            foreach (var e in _gameScheduler.WaitingSimulations)
            {
                GD.Print($"{e.GetType()}: {e.Layer}");
            }
        }
        if (ImGui.Button("添加调度事件"))
        {
            _event0 = EventFactory.CreateEvent((s, a, r) =>
                {
                    s.ApplyDamage(new(3, 3),
                        10, r);
                });
            _gameScheduler.AddEvent(_event0);
        }

        if (ImGui.Button("触发调度事件"))
        {
            (_event0 as ISchedulerEvnet)?.Layer = SimulationLayer.Running;
        }

        if (ImGui.Button("测试放置"))
        {

            _gameScheduler.AddEvent(EventFactory.CreateEvent((s, a, r) =>
            {
                for (int i = 0; i < 10; i++)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        s.ApplyPlaceTile(new(i, j), new TileState()
                        {
                            Tag = TileTag.Ground
                        }, r);
                    }
                }
                s.ApplyPlaceUnit(new(2, 2), UnitFactory.CreateUnit("Unit_0"), r);
            }), true);
        }
        if (ImGui.Button("测试放置2"))
        {

            _event2 = _gameScheduler.AddEvent(EventFactory.CreateEvent((s, a, r) =>
            {
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        s.ApplyPlaceUnit(new(i,j), UnitFactory.CreateUnit("Unit_0"), r);
                    }
                }
            }));
        }

        if (ImGui.Button("使用事件2"))
        {
            (_event2 as ISchedulerEvnet)?.Layer = SimulationLayer.Running;
        }
        
        if (ImGui.Button("测试移动2,2 -> 6,3"))
        {

            _move = _gameScheduler.AddEvent(EventFactory.CreateEvent((s, a, r) =>
            {
                 s.ApplyMoveUnit(new(2,2), new(6,3), r);
                s.ApplyMoveUnit(new(6,3), new(1,4), r);
            }));
        }

        if (ImGui.Button("使用移动"))
        {
            (_move as ISchedulerEvnet)?.Layer = SimulationLayer.Running;
        }

        if (ImGui.Button("改变移动的关注性"))
        {
            (_move as ISchedulerEvnet)?.Layer = _move.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }
        
        if (ImGui.Button("测试伤害"))
        {

            _damage = _gameScheduler.AddEvent(EventFactory.CreateEvent((s, a, r) =>
            {
                 s.ApplyDamage(new(2,2), 50, r);
            }));
        }

        if (ImGui.Button("使用伤害"))
        {
            (_damage as ISchedulerEvnet)?.Layer = SimulationLayer.Running;
        }
        if (ImGui.Button("改变伤害的关注性"))
        {
            (_damage as ISchedulerEvnet)?.Layer = _damage.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }

        if (ImGui.Button("添加跟随技能"))
        {
            _follow = _gameScheduler.AddEvent(EventFactory.CreateSkill(1, Skills.Skill1));
        }
        if (ImGui.Button("改变跟随的关注性"))
        {
            (_follow as ISchedulerEvnet)?.Layer = _follow.Layer == SimulationLayer.Focused ? SimulationLayer.Waiting : SimulationLayer.Focused;
        }
        ImGui.End();
        
    }
}