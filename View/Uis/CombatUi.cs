using System.ComponentModel;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.View.Uis;

public partial class CombatUi : Control
{
    [Export] public GameView GameView { get; set; }
    [Export] private InfoPanel _infoPanel;
    [Export] private PackedScene _itemBoxScene;
    [Export] private Control _skillContainer;
    [Export] private Control _buffContainer;
    [Export] private TooltipManager _tooltipManager;
    [Export] private Button _stopTurnButton;
    private IPlayer _player;
    private GameScheduler _gameScheduler;
    public override void _Ready()
    {
        base._Ready();
        _tooltipManager.OnHoverChange += OnHoverChange;
    }

    public void Bind(GameScheduler gameScheduler, IPlayer player)
    {
        _gameScheduler = gameScheduler;
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
            var player = sender as IPlayer;
            if (e.PropertyName is nameof(IPlayer.SkillingUnitSkillStates))
            {
                ClearSkillItem();
                foreach (var skill in _player.SkillingUnitSkillStates)
                {
                    AddSkillItem(skill);
                }
            }
            if (e.PropertyName is nameof(IPlayer.SelectedUnitId))
            {
                var unit = _gameScheduler.GameState.UnitMapState.GetUnit(player.SelectedUnitId);
                ClearBuffItem();
                foreach (var buff in unit.Buffs)
                {
                    AddBuffItem(buff.BuffType);
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

    private void OnHoverChange(object sender, object hoverObject, bool isHover)
    {
        _infoPanel.Visible = isHover;
        var itemBox = (hoverObject as ItemBox)!;
        //设置位置
        if (isHover)
        {
            _infoPanel.IconTexture = (itemBox.MetaData as IBuff)?.Info.Icon;
            _infoPanel.TitleName = (itemBox.MetaData as IBuff)?.Info.BuffType.ToString() ?? "";
            _infoPanel.Description = (itemBox.MetaData as IBuff)?.Info.Description ?? "";
            
            var pos = GetLocalMousePosition();
            _infoPanel.LeftDownPosition = pos;
        }
    }

    public void AddSkillItem(SkillState skillState)
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _skillContainer.AddChild(itemBox);
        itemBox.MetaData = skillState.Skill;
        itemBox.MouseEntered += () => _tooltipManager.MouseEnter(itemBox);
        itemBox.MouseExited += () => _tooltipManager.MouseExit(itemBox);
    }

    public void ClearSkillItem()
    {
        var count = _skillContainer.GetChildCount();
        for (var i = count - 1; i >= 0; i--)
        {
            _skillContainer.GetChild(i).QueueFree();
        }
    }
    
    public void AddBuffItem(BuffTypeEnum buffTypeEnum)
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _buffContainer.AddChild(itemBox);
        itemBox.Texture = buffTypeEnum.GetInstance().Info.Icon;
        itemBox.MetaData = buffTypeEnum.GetInstance();
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