using System.ComponentModel;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Views.Uis;

public partial class CombatUi : Control
{
    public IGameView GameView { get; set; } = null!;
    [ExportGroup("Self")]
    [Export] private InfoPanel _infoPanel = null!;
    [Export] private PackedScene _itemBoxScene = null!;
    [Export] private Control _skillContainer = null!;
    [Export] private Control _buffContainer = null!;
    [Export] private Control _unitPanel = null!;
    [Export] private TooltipManager _tooltipManager = null!;
    [Export] private Button _stopTurnButton = null!;
    [Export] private TextureRect _unitPreviewRect = null!;
    [Export] private Viewport _unitPreviewViewport = null!;
    [Export] private Camera2D _unitPreviewCamera = null!;
    [Export] private Control _titleTip = null!;
    [Export] private Label _titleTipLabel = null!;
    private IPlayer _player = null!;
    private IEventScheduler _eventScheduler = null!;
    private long _selectedUnitId = 0;
    
    private Tween? _titleTipTween;
    public override void _Ready()
    {
        base._Ready();
        _tooltipManager.OnHoverChange += OnHoverChange;
        _unitPreviewViewport.World2D = GetViewport().World2D;
    }

    public override void _Process(double delta)
    {
        var uv = GameView.GetUnitView(_selectedUnitId);
        if (uv == null) return;
        _unitPreviewCamera.GlobalPosition = uv.GlobalPosition;
    }

    public void Bind(IEventScheduler eventScheduler, IGameView gameView, IPlayer player)
    {
        GameView = gameView;
        _eventScheduler = eventScheduler;
        _player = player;
        
        _player.PropertyChanged += PlayerOnPropertyChanged;



        _stopTurnButton.MouseEntered += StopTurnButtonOnMouseEntered;
        _stopTurnButton.MouseExited += StopTurnButtonOnMouseExited;
        _stopTurnButton.Pressed += StopTurnButtonOnPressed;

        TreeExiting += () =>
        {
            _player.PropertyChanged -= PlayerOnPropertyChanged;
        };
        
        void PlayerOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var player = (sender as IPlayer)!;
            if (e.PropertyName is nameof(IPlayer.SelectedUnitId))
            {
                _selectedUnitId = player.SelectedUnitId;
                ClearSkillItem();
                
                _unitPanel.Visible = _selectedUnitId != 0;
                if (_selectedUnitId != 0)
                {
                    var skills =
                        GameView.GameState.GetUnitValidSkills(
                            GameView.GameState.UnitMapState.GetUnit(player.SelectedUnitId).Name);
                    for (var index = 0; index < skills.Count; index++)
                    {
                        var skill = skills[index];
                        AddSkillItem(skill.SkillName.GetSkillInfo(), index);
                    }
                }
            }
            if (e.PropertyName is nameof(IPlayer.SelectedUnitId))
            {
                var unit = _eventScheduler.GameState.UnitMapState.GetUnit(player.SelectedUnitId);
                ClearBuffItem();
                foreach (var buff in unit.Buffs)
                {
                    AddBuffItem(buff);
                }
            }

            if (e.PropertyName is nameof(IPlayer.IsTurn))
            {
                ShowTitleTip(player.IsTurn ? "你的回合" : "回合结束", 1.0f);
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
            _infoPanel.IconTexture = (itemBox.MetaData as IBuff)?.Name.GetBuffInfo().Icon;
            _infoPanel.TitleName = (itemBox.MetaData as IBuff)?.Name ?? string.Empty;
            _infoPanel.Description = (itemBox.MetaData as IBuff)?.Name.GetBuffInfo().Description ?? "";
            
            var pos = GetLocalMousePosition();
            _infoPanel.LeftDownPosition = pos;
        }
    }

    public void AddSkillItem(SkillInfo skill, int index)
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _skillContainer.AddChild(itemBox);
        itemBox.MetaData = skill;
        itemBox.MouseEntered += () => _tooltipManager.MouseEnter(itemBox);
        itemBox.MouseExited += () => _tooltipManager.MouseExit(itemBox);
        itemBox.Click += () => _player.OnInput(new PlayerInputSkill(index));
    }

    public void ClearSkillItem()
    {
        var count = _skillContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            _skillContainer.GetChild(i).QueueFree();
        }
    }
    
    public void AddBuffItem(in BuffState state)
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _buffContainer.AddChild(itemBox);
        itemBox.Texture = state.BuffName.GetBuffInfo().Icon!;
        itemBox.MetaData = state.BuffName.GetBuffInfo().Hook;
        itemBox.HasCount = true;
        itemBox.Count = state.Count;
        itemBox.MouseEntered += () => _tooltipManager.MouseEnter(itemBox);
        itemBox.MouseExited += () => _tooltipManager.MouseExit(itemBox);
    }

    public void ClearBuffItem()
    {
        var count = _buffContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            _buffContainer.GetChild(i).QueueFree();
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