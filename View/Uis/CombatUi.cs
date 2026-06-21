using System.ComponentModel;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.View.Uis;

public partial class CombatUi : Control
{
    public IGameView GameView { get; set; } = null!;
    [ExportGroup("Self")]
    [Export] private InfoPanel _infoPanel = null!;
    [Export] private PackedScene _itemBoxScene = null!;
    [Export] private Control _skillContainer = null!;
    [Export] private Control _buffContainer = null!;
    [Export] private TooltipManager _tooltipManager = null!;
    [Export] private Button _stopTurnButton = null!;
    private IPlayer _player = null!;
    private EventScheduler _eventScheduler = null!;
    public override void _Ready()
    {
        base._Ready();
        _tooltipManager.OnHoverChange += OnHoverChange;
    }

    public void Bind(EventScheduler eventScheduler, IGameView gameView, IPlayer player)
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
            if (e.PropertyName is nameof(IPlayer.SkillingUnitSkillStates))
            {
                ClearSkillItem();
                for (var index = 0; index < _player.SkillingUnitSkillStates.Count; index++)
                {
                    var skill = _player.SkillingUnitSkillStates[index];
                    AddSkillItem(skill, index);
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

    public void AddSkillItem(SkillState skillState, int index)
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _skillContainer.AddChild(itemBox);
        itemBox.MetaData = skillState.Skill;
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
}