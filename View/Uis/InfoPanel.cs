
namespace TheBlockWar.View.Uis;

public partial class InfoPanel : PanelContainer
{
    [Export] private TextureRect _iconTextureRect;
    [Export] private Label _nameLabel;
    [Export] private Label _descriptionLabel;
    [Export] private TextureRect _useTextureRect;
    public Texture2D IconTexture { get => _iconTextureRect.Texture; set => _iconTextureRect.Texture = value; }
    public string TitleName { get => _nameLabel.Text; set => _nameLabel.Text = value; }
    public string Description { get => _descriptionLabel.Text; set => _descriptionLabel.Text = value; }

    public Texture2D? UseTexture
    {
        get => _useTextureRect.Texture;
        set
        { 
            _useTextureRect.Texture = value;
            _useTextureRect.Visible = value != null;
        }
    }
}