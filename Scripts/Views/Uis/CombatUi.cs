using System.ComponentModel;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.Uis;

public partial class CombatUi : Control
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();
    public IGameView GameView { get; set; } = null!;
    [ExportGroup("Self")]
    [Export] private InfoPanel _infoPanel = null!;
    [Export] private PackedScene _itemBoxScene = null!;
    [Export] private Control _skillContainer = null!;
    [Export] private Control _buffContainer = null!;
    [Export] private Control _regionContainer = null!;
    [Export] private Control _unitPanel = null!;
    [Export] private Control _tilePanel = null!;
    [Export] private TooltipManager _tooltipManager = null!;
    [Export] private Button _stopTurnButton = null!;
    [Export] private TextureRect _unitPreviewRect = null!;
    [Export] private TextureRect _tilePreviewRect = null!;
    [Export] private Viewport _unitPreviewViewport = null!;
    [Export] private Camera2D _unitPreviewCamera = null!;
    [Export] private Control _titleTip = null!;
    [Export] private Label _titleTipLabel = null!;
    [Export] private Slider _healthSlider = null!;
    [Export] private Slider _shieldSlider = null!;
    [Export] private Label _healthLabel = null!;
    [Export] private Label _shieldLabel = null!;
    [Export] private Label _turnLabel = null!;
    [Export] private Label _phaseTitleLabel = null!;
    [Export] private Label _tileXLabel = null!;
    [Export] private Label _tileYLabel = null!;
    private IPlayer _player = null!;
    private IEventScheduler _eventScheduler = null!;
    private ITurnManager _turnManager = null!;
    private long _selectedUnitId = 0;
    private TypedPool<ItemBox> _itemBoxPool;
    
    private Tween? _titleTipTween;
    public override void _Ready()
    {
        base._Ready();
        _tooltipManager.OnHoverChange += OnHoverChange;
        _unitPreviewViewport.World2D = GetViewport().World2D;
        _buffContainer.FreeChildren();
        _regionContainer.FreeChildren();
        _skillContainer.FreeChildren();
        
        _itemBoxPool = new TypedPool<ItemBox>(() =>
        {
            var itemBox = _itemBoxScene.Instantiate<ItemBox>();
            itemBox.MouseEntered += () => _tooltipManager.MouseEnter(itemBox);
            itemBox.MouseExited += () => _tooltipManager.MouseExit(itemBox);
            return itemBox;
        }, 10, null, r =>
        {
            r.GetParent()?.RemoveChild(r);
            r.Reset();
        });
    }

    public override void _Process(double delta)
    {
        var uv = GameView.GetUnitView(_selectedUnitId);
        if (uv == null) return;
        _unitPreviewCamera.GlobalPosition = uv.GlobalPosition;
    }

    public void Bind(IEventScheduler eventScheduler, ITurnManager turnManager, IGameView gameView, IPlayer player)
    {
        GameView = gameView;
        _eventScheduler = eventScheduler;
        _player = player;
        _turnManager = turnManager;
        
        _player.PropertyChanged += PlayerOnPropertyChanged;
        eventScheduler.SimulationUpdated += EventSchedulerOnSimulationUpdated;
        turnManager.PhaseChanged += PhaseManagerOnPhaseChanged;


        _stopTurnButton.MouseEntered += StopTurnButtonOnMouseEntered;
        _stopTurnButton.MouseExited += StopTurnButtonOnMouseExited;
        _stopTurnButton.Pressed += StopTurnButtonOnPressed;

        TreeExiting += () =>
        {
            _player.PropertyChanged -= PlayerOnPropertyChanged;
            _eventScheduler.SimulationUpdated -= EventSchedulerOnSimulationUpdated;
            _turnManager.PhaseChanged -= PhaseManagerOnPhaseChanged;
        };
        
        void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(IPlayer.SelectedUnitId))
            {
                _selectedUnitId = player.SelectedUnitId;
                _unitPanel.Visible = _selectedUnitId != 0;
                if (_unitPanel.Visible)
                {
                    UpdateSkillTab();
                    UpdateBuffTab();
                    UpdateValues();
                }
            }
            else if (e.PropertyName is nameof(IPlayer.IsTurn))
            {
                ShowTitleTip(player.IsTurn ? "你的回合" : "回合结束", 1.0f);
            }
            else if (e.PropertyName is nameof(IPlayer.SelectedSkillIndex))
            {
                var count = _skillContainer.GetChildCount();
                for (var i = 0; i < count; i++)
                {
                    var item = _skillContainer.GetChild(i) as ItemBox;
                    item?.IsSelected = i == player.SelectedSkillIndex;
                }
            }
            else if (e.PropertyName is nameof(IPlayer.SelectedTileCoords))
            {
                _tilePanel.Visible = player.SelectedTileCoords != null;
                if (_tilePanel.Visible)
                {
                    UpdateRegionTab();
                    UpdateTilePreview();
                    _tileXLabel.Text = $"{player.SelectedTileCoords!.Value.X}";
                    _tileYLabel.Text = $"{player.SelectedTileCoords.Value.Y}";
                }
                
            }
        }
        void StopTurnButtonOnPressed()
        {
            if (GameView.IsRunning) return;
            _player.OnInput(new PlayerInputStopTurn(PlayerSotpTurnEnum.Click));
        }

        void StopTurnButtonOnMouseExited()
        {
            if (GameView.IsRunning) return;
            _player.OnInput(new PlayerInputStopTurn(PlayerSotpTurnEnum.HoverExit));
        }
        void StopTurnButtonOnMouseEntered()
        {
            if (GameView.IsRunning) return;
            _player.OnInput(new PlayerInputStopTurn(PlayerSotpTurnEnum.HoverEnter));
        }
    }

    private void PhaseManagerOnPhaseChanged(object? sender, (int index, bool isEnter, ITurnPhase phase) e)
    {
        if (e.isEnter)
        {
            if (e.index == 0)
            {
                _turnLabel.Text = $"{_turnManager.CurrentTurn}";
            }
            
            var selfPhase = e.phase is PlayerPhase ph && ph.Player.Id == _player.Id;
            _phaseTitleLabel.Text = selfPhase ? "你的阶段:" : "当前阶段:";
            _stopTurnButton.Disabled = !selfPhase;
        }
    }

    private void EventSchedulerOnSimulationUpdated(object? sender, SchedulerResult e)
    {
        if (e.RunningCount > 0 && _unitPanel.Visible)
        {
            UpdateValues();
            UpdateBuffTab();
            UpdateSkillTab();
            UpdateRegionTab();
            UpdateTilePreview();
        }
    }

    private void UpdateValues()
    {
        var unit = _eventScheduler.GameState.UnitMapState.GetUnit(_player.SelectedUnitId);
        _healthSlider.MaxValue = unit.MaxHealth;
        _healthSlider.Value = unit.Health;
        _shieldSlider.MaxValue = unit.MaxShield;
        _shieldSlider.Value = unit.Shield;
        _healthLabel.Text = $"{unit.Health}/{unit.MaxHealth}";
        _shieldLabel.Text = $"{unit.Shield}/{unit.MaxShield}";
    }

    private void UpdateTilePreview()
    {
        var coords = _player.SelectedTileCoords;
        if (coords == null) return;
        var info = _eventScheduler.GameState.TileMapState.GetTile(coords.Value).Name.GetTileInfo();
        _tilePreviewRect.Texture = info.Icon;
    }
    private void UpdateBuffTab()
    {
        var unit = _eventScheduler.GameState.UnitMapState.GetUnit(_player.SelectedUnitId);
        ClearBuffItem();
        foreach (var buff in unit.Buffs)
        {
            AddBuffItem(buff);
        }
    }

    private void UpdateSkillTab()
    {
        ClearSkillItem();
        if (_selectedUnitId != 0)
        {
            var unit = _eventScheduler.GameState.UnitMapState.GetUnit(_selectedUnitId);
            var skills =
                GameView.GameState.GetUnitValidSkills(
                    GameView.GameState.UnitMapState.GetUnit(_player.SelectedUnitId).Name);
            for (var index = 0; index < skills.Count; index++)
            {
                var skill = skills[index];
                var disabled = !unit.GetShouldUseSkill(_eventScheduler.GameState, skill) || unit.Team != _player.Team;
                AddSkillItem(skill.SkillName.GetSkillInfo(), index, 
                    index == _player.SelectedSkillIndex, disabled);
            }
        }
    }
    private void UpdateRegionTab()
    {
        ClearRegionItem();
        var coords = _player.SelectedTileCoords;
        if (coords == null) return;
        foreach (var region in _eventScheduler.GameState.TileMapState.GetTile(coords.Value).Regions)
        {
            AddRegionItem(region);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed)
        {
            int num = (int)((long)key.Keycode - (long)Key.Key1);
            _player.OnInput(new PlayerInputSkill(num));
        }
    }

    private void OnHoverChange(object sender, object? hoverObject, bool isHover)
    {
        _infoPanel.Visible = isHover;
        var itemBox = (hoverObject as ItemBox)!;
        //设置位置
        if (isHover)
        {
            if (itemBox.MetaData is BuffInfo buff)
            {
                _infoPanel.IconTexture = buff.Icon;
                _infoPanel.TitleName = buff.Name;
                _infoPanel.Description = buff.Description;
            }
            else if (itemBox.MetaData is SkillInfo skill)
            {
                _infoPanel.IconTexture = skill.Icon;
                _infoPanel.TitleName = skill.Name;
                _infoPanel.Description = skill.Description;
            }
            else if (itemBox.MetaData is RegionInfo region)
            {
                _infoPanel.IconTexture = region.Icon;
                _infoPanel.TitleName = region.Name;
                _infoPanel.Description = region.Description;
            }
            var pos = GetLocalMousePosition();
            _infoPanel.LeftDownPosition = pos;
        }
    }
    public void AddRegionItem(in RegionState region)
    {
        var itemBox = _itemBoxPool.Get();
        _regionContainer.AddChild(itemBox);
        var info = region.RegionName.GetRegionInfo();
        itemBox.Texture = info.Icon!;
        itemBox.HasCount = region.Duration < 100;
        itemBox.Count = region.Duration;
        itemBox.MetaData = info;
    }
    public void ClearRegionItem()
    {
        var count = _regionContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            var itemBox = _regionContainer.GetChild(i) as ItemBox;
            _itemBoxPool.Return(itemBox!);
        }
    }

    public void AddSkillItem(SkillInfo skill, int index, bool isSelected = false, bool disabled = false)
    {
        var itemBox = _itemBoxPool.Get();
        _skillContainer.AddChild(itemBox);
        itemBox.Disabled = disabled;
        itemBox.MetaData = skill;
        itemBox.Texture = skill.Icon;
        itemBox.IsSelected = isSelected;
        itemBox.Click += () => _player.OnInput(new PlayerInputSkill(index));
    }

    public void ClearSkillItem()
    {
        var count = _skillContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            var itemBox = _skillContainer.GetChild(i) as ItemBox;
            _itemBoxPool.Return(itemBox!);
        }
    }
    
    public void AddBuffItem(in BuffState state)
    {
        var itemBox = _itemBoxPool.Get();
        _buffContainer.AddChild(itemBox);
        itemBox.Texture = state.BuffName.GetBuffInfo().Icon!;
        itemBox.MetaData = state.BuffName.GetBuffInfo();
        itemBox.HasCount = true;
        itemBox.Count = state.Count;
    }

    public void ClearBuffItem()
    {
        var count = _buffContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            var itemBox = _buffContainer.GetChild(i) as ItemBox;
            _itemBoxPool.Return(itemBox!);
        }
    }

    public void ShowTitleTip(string title, float time)
    {
        // 如果已有动画，先杀死
        if (_titleTipTween != null)
        {
            _titleTipTween.Kill();
            _titleTipTween = null;
        }

        _titleTipLabel.Text = title;
        _titleTip.Visible = true;
        _titleTip.Modulate = new Color(1, 1, 1, 0); // 初始透明

        _titleTipTween = CreateTween();
        _titleTipTween.SetTrans(Tween.TransitionType.Linear);
        _titleTipTween.SetEase(Tween.EaseType.InOut);

        // 快速淡入（0.1 秒变为不透明）
        _titleTipTween.TweenProperty(_titleTip, "modulate:a", 1.0, 0.1);
    
        // 停留显示
        _titleTipTween.TweenInterval(time);
    
        // 快速淡出（0.1 秒变为透明）
        _titleTipTween.TweenProperty(_titleTip, "modulate:a", 0.0, 0.1);
    
        // 动画结束隐藏并清理
        _titleTipTween.Finished += () =>
        {
            _titleTip.Visible = false;
            _titleTipTween = null;
        };
    }
}