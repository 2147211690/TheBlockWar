namespace TheBlockWar.Scripts.Views.Uis;

public partial class PlayerInfoItem : PanelContainer
{
    public string Name {get => NameLabel.Text; set => NameLabel.Text = value; }
    public long Id {get => long.Parse(IdLabel.Text); set => IdLabel.Text = value.ToString(); }
    public Texture2D HeaderTexture
    {
        get => HeaderTextureRect.Texture; 
        set => HeaderTextureRect.Texture = value; 
    }
    
    [ExportGroup("Self")]
    [Export] public Label NameLabel { get; set; }
    [Export] public Label IdLabel { get; set; }
    [Export] public TextureRect HeaderTextureRect { get; set; }
}