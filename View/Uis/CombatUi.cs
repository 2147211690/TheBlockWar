using TheBlockWar.Scripts.Games;
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
        
        
        _stopTurnButton.MouseEntered += StopTurnButtonOnMouseEntered;
        _stopTurnButton.MouseExited += StopTurnButtonOnMouseExited;
        _stopTurnButton.Pressed += StopTurnButtonOnPressed;

        TreeExiting += () =>
        {
        };
        
        
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

    private void OnHoverChange(object sender, object? hoverObject, bool isHover)
    {
        _infoPanel.Visible = isHover;
        //设置位置
        var viewSize = GetViewportRect().Size;
        var pos = GetLocalMousePosition() + new Vector2(0, -_infoPanel.Size.Y);
        if (pos.X > viewSize.X - _infoPanel.Size.X) pos.X = viewSize.X - _infoPanel.Size.X;
        if (pos.Y < _infoPanel.Size.Y) pos.Y = _infoPanel.Size.Y;
        _infoPanel.Position = pos;
    }

    public void AddSkillItem()
    {
        var itemBox = _itemBoxScene.Instantiate<ItemBox>();
        _skillContainer.AddChild(itemBox);
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
}