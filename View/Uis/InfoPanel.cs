
namespace TheBlockWar.View.Uis;

public partial class InfoPanel : CenterContainer
{
    [Export] private TextureRect _iconTextureRect = null!;
    [Export] private Label _nameLabel = null!;
    [Export] private Label _descriptionLabel = null!;
    [Export] private TextureRect _useTextureRect = null!;
    private Control _child = null!;
    public Vector2 LeftDownPosition
    {
        get => field;
        set
        { 
            field = value;
            UpdatePosition();
        }
    }

    public Texture2D? IconTexture
    {
        get => _iconTextureRect.Texture;
        set => _iconTextureRect.Texture = value;
    }

    public string TitleName
    {
        get => _nameLabel.Text;
        set => _nameLabel.Text = value;
    }

    public string Description
    {
        get => _descriptionLabel.Text;
        set => _descriptionLabel.Text = value;
    }

    public Texture2D? UseTexture
    {
        get => _useTextureRect.Texture;
        set
        {
            _useTextureRect.Texture = value;
            _useTextureRect.Visible = value != null;
        }
    }

    public override void _Ready()
    {
        base._Ready();
        _child = GetChild<Control>(0);
        _child.Resized += UpdatePosition;
    }
    private void UpdatePosition()
    {
        var pos = LeftDownPosition;
        var viewSize = GetViewportRect().Size;
        var contentSize = _child.Size;
        if (pos.X + contentSize.X > viewSize.X) pos.X = viewSize.X - contentSize.X;
        if (pos.Y - contentSize.Y < -viewSize.Y) pos.Y = contentSize.Y - viewSize.Y;
        Position = pos + new Vector2(_child.Size.X / 2, -_child.Size.Y / 2);
    }
}
