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
    private IPlayer _player;
    private GameScheduler _gameScheduler;
    public override void _Ready()
    {
        base._Ready();
        _tooltipManager.OnHoverChange += OnHoverChange;
    }

    public void Bind(GameScheduler gameScheduler, IPlayer player)
    {
        
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