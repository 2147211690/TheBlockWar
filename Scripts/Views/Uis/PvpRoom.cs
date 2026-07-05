using Godot;
using System;
using System.Linq;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.Scenes;

namespace TheBlockWar.Scripts.Views.Uis;
public partial class PvpRoom : Control
{
    [ExportGroup("Self")]
    [Export] public PackedScene PvpScene { get; set; } = null!;
    [Export] public LineEdit AddressText { get; set; } = null!;
    [Export] public LineEdit PortText { get; set; } = null!;
    [Export] public LineEdit NameText { get; set; } = null!;
    [Export] public Button CreateButton { get; set; } = null!;
    [Export] public Button JoinButton { get; set; } = null!;
    [Export] public Button ExitButton { get; set; } = null!;
    [Export] public Button StartButton { get; set; } = null!;
    [Export] public Button LoadingButton { get; set; } = null!;
    [Export] public Control PlayerInfoItemContainer { get; set; } = null!;
    [Export] public PackedScene PlayerInfoItemPrefab { get; set; } = null!;
    public OnlineRoom OnlineRoom { get; set; } = null!;
    
    private Dictionary<long, Control> _playerItemControls = new();
    public State CurrentState
    {
        get => field;
        private set
        {
            if (field == value) return;
            field = value;
            UpdateView();
        }
    }

    public enum State
    {
        Free,
        MyRoom,
        OtherRoom,
        Loading,
    }
    
    public override void _Ready()
    {
        CallHelper.QueueCall(this, _ =>
        {
            CreateButton.Pressed += CreateButton_Pressed;
            JoinButton.Pressed += async () => await JoinButton_Pressed();
            ExitButton.Pressed += ExitButton_Pressed;
            StartButton.Pressed += StartButton_Pressed;
            OnlineRoom = new();
            GetTree().Root.AddChild(OnlineRoom);
            OnlineRoom.PlayerAdded += PlayerAdded;
            OnlineRoom.PlayerRemoved += PlayerRemoved;
            OnlineRoom.GameStart += OnlineRoomOnGameStart;
            OnlineRoom.RoomClosed += RoomClosed;
        });
        
        UpdateView();
    }

    private void RoomClosed()
    {
        CurrentState = State.Free;
    }

    private void OnlineRoomOnGameStart()
    {
        var players = OnlineRoom.Players.Select(p => (p.Key, p.Value)).ToArray();
        var netId = OnlineRoom.NetId;
        var peer = OnlineRoom.Peer;
        SceneManager.ChangeSceneToPacked(PvpScene, p =>
        {
            ((Pvp)p).StartGame(players,1, netId, peer);
        });
    }

    private void PlayerRemoved(long obj)
    {
        if (!_playerItemControls.TryGetValue(obj, out var item)) return;
        item.QueueFree();
        _playerItemControls.Remove(obj);
    }

    private void PlayerAdded(long arg1, PlayerInfo arg2)
    {
        var item = PlayerInfoItemPrefab.Instantiate() as TheBlockWar.Scripts.Views.Uis.PlayerInfoItem;
        PlayerInfoItemContainer.AddChild(item);
        item.Name = arg2.Name;
        item.Id = arg1;
        item.HeaderTexture = null;
        _playerItemControls.Add(arg1, item);
    }


    private void CreateButton_Pressed()
    {
        if (!int.TryParse(PortText.Text, out var port)) return;
        UpdatePlayerInfo();
        OnlineRoom.Port = port;
        if (OnlineRoom.CreateRoom())
        {
            CurrentState = State.MyRoom;
        }
    }
    
    
    private async GDTask JoinButton_Pressed()
    {
        CurrentState = State.Loading;
        if (!int.TryParse(PortText.Text, out var port)) return;
        UpdatePlayerInfo();
        var r = await OnlineRoom.JoinRoom(AddressText.Text, port);
        if (r)
        {
            CurrentState = State.OtherRoom;
        }
        else
        {
            CurrentState = State.Free;
        }
    }
    
    
    private void ExitButton_Pressed()
    {
        OnlineRoom.CloseRoom(); 
    }
    
    
    private void StartButton_Pressed()
    {
        OnlineRoom.StartGame();
    }

    private void UpdatePlayerInfo()
    {
        OnlineRoom.SelfInfo.Name = NameText.Text;
    }
    private void UpdateView()
    {
        if (CurrentState == State.Free)
        {
            CreateButton.Visible = true;
            JoinButton.Visible = true;
            ExitButton.Visible = false;
            StartButton.Visible = false;
            LoadingButton.Visible = false;
        }
        else if (CurrentState == State.MyRoom)
        {
            CreateButton.Visible = false;
            JoinButton.Visible = false;
            ExitButton.Visible = true;
            StartButton.Visible = true;
            LoadingButton.Visible = false;
        }
        else if (CurrentState == State.OtherRoom)
        {
            CreateButton.Visible = false;
            JoinButton.Visible = false;
            ExitButton.Visible = true;
            StartButton.Visible = false;
            LoadingButton.Visible = false;
        }
        else if (CurrentState == State.Loading)
        {
            CreateButton.Visible = false;
            JoinButton.Visible = false;
            ExitButton.Visible = false;
            StartButton.Visible = false;
            LoadingButton.Visible = true;
        }
    }
}
