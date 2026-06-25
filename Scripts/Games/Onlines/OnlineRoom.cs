using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools.Jsons;

namespace TheBlockWar.Scripts.Games.Onlines;

[GlobalClass]
public partial class OnlineRoom : Node
{
    private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    [Export] public long NetId { get; private set; }
    public PlayerInfo SelfInfo { get; set; } = new();
    [Export] public int Port { get; set; } = 8910;

    public MultiplayerPeer Peer
    {
        get => GetTree().Root.Multiplayer.MultiplayerPeer;
        set => GetTree().Root.Multiplayer.MultiplayerPeer = value;
    }

    private readonly Dictionary<long, PlayerInfo> _players = [];

    public IReadOnlyDictionary<long, PlayerInfo> Players => _players;
    public bool IsHost => Multiplayer.IsServer();

    public event Action<long, PlayerInfo>? PlayerAdded;
    public event Action<long>? PlayerRemoved;
    public event Action? RoomClosed;
    public event Action? GameStart;

    public override void _Ready()
    {
        Multiplayer.PeerConnected += OnPeerConnected;
        Multiplayer.PeerDisconnected += OnPeerDisconnected;
        Multiplayer.ConnectedToServer += OnConnectedToServer;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ServerDisconnected += OnServerDisconnected;

        TreeExiting += () =>
        {
            CloseRoom();
            Multiplayer.PeerConnected -= OnPeerConnected;
            Multiplayer.PeerDisconnected -= OnPeerDisconnected;
            Multiplayer.ConnectedToServer -= OnConnectedToServer;
            Multiplayer.ConnectionFailed -= OnConnectionFailed;
            Multiplayer.ServerDisconnected -= OnServerDisconnected;
        };
    }

    public bool CreateRoom()
    {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateServer(Port);
        if (error != Error.Ok)
        {
            Logger.Error("创建房间失败: {error}", error);
            return false;
        }
        Peer = peer;
        NetId = Multiplayer.GetUniqueId();
        _players[NetId] = SelfInfo;
        Logger.Info("房间创建成功, NetId: {NetId}, Port: {Port}", NetId, Port);
        PlayerAdded?.Invoke(NetId, SelfInfo);
        return true;
    }

    public async GDTask<bool> JoinRoom(string address, int port)
    {
        var peer = new ENetMultiplayerPeer();
        var error = peer.CreateClient(address, port);
        if (error != Error.Ok)
        {
            Logger.Error("连接房间失败: {error}", error);
            return false;
        }
        Peer = peer;

        var tcs = new GDTaskCompletionSource<bool>();

        void OnConnected()
        {
            NetId = Multiplayer.GetUniqueId();
            tcs.TrySetResult(true);
        }

        void OnFailed()
        {
            tcs.TrySetResult(false);
        }

        Multiplayer.ConnectedToServer += OnConnected;
        Multiplayer.ConnectionFailed += OnFailed;

        var result = await tcs.Task;

        Multiplayer.ConnectedToServer -= OnConnected;
        Multiplayer.ConnectionFailed -= OnFailed;

        return result;
    }

    public void CloseRoom()
    {
        if (Peer is not null)
        {
            Peer.Close();
            Peer = null;
        }
        foreach (var id in _players.Keys)
            PlayerRemoved?.Invoke(id);
        _players.Clear();
        NetId = 0;
        RoomClosed?.Invoke();
        Logger.Info("房间已关闭");
    }

    public void StartGame()
    {
        Rpc(MethodName.RpcStartGame);
    }

    private void OnPeerConnected(long id)
    {
        Logger.Info("Peer 连接: {id}", id);
        if (!IsHost) return;
        SyncPlayersToPeer(id);
    }

    private void OnPeerDisconnected(long id)
    {
        Logger.Info("Peer 断开: {id}", id);
        if (!IsHost) return;
        _players.Remove(id);
        PlayerRemoved?.Invoke(id);
        Rpc(MethodName.RpcRemovePlayer, id);
    }

    private void OnConnectedToServer()
    {
        NetId = Multiplayer.GetUniqueId();
        Logger.Info("已连接到服务器, NetId: {NetId}", NetId);
        RegisterSelfToServer();
    }

    private void OnConnectionFailed()
    {
        Logger.Error("连接服务器失败");
    }

    private void OnServerDisconnected()
    {
        Logger.Info("服务器断开连接");
        foreach (var id in _players.Keys)
            PlayerRemoved?.Invoke(id);
        RoomClosed?.Invoke();
        _players.Clear();
        NetId = 0;
    }

    private void SyncPlayersToPeer(long peerId)
    {
        var data = new Dictionary<string, string>();
        foreach (var (id, info) in _players)
            data[id.ToString()] = info.Name;
        var json = GameJson.ToJson(data);
        RpcId(peerId, MethodName.RpcSyncAllPlayers, json);
    }

    private void RegisterSelfToServer()
    {
        RpcId(1, MethodName.RpcRegisterPlayer, SelfInfo.Name);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcSyncAllPlayers(string playersJson)
    {
        var data = GameJson.FromJson<Dictionary<string, string>>(playersJson);
        if (data is null) return;
        _players.Clear();
        foreach (var (idStr, name) in data)
        {
            var id = long.Parse(idStr);
            var info = new PlayerInfo { Name = name };
            _players[id] = info;
            PlayerAdded?.Invoke(id, info);
        }
        Logger.Info("玩家列表已同步, 共 {count} 人", _players.Count);
    }

    [Rpc(MultiplayerApi.RpcMode.AnyPeer, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRegisterPlayer(string name)
    {
        var senderId = Multiplayer.GetRemoteSenderId();
        var playerInfo = new PlayerInfo { Name = name };
        _players[senderId] = playerInfo;
        PlayerAdded?.Invoke(senderId, playerInfo);
        Logger.Info("玩家加入: [{senderId}] {name}", senderId, name);
        Rpc(MethodName.RpcAddPlayer, senderId, name);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcAddPlayer(long peerId, string name)
    {
        if (_players.ContainsKey(peerId)) return;
        var playerInfo = new PlayerInfo { Name = name };
        _players[peerId] = playerInfo;
        PlayerAdded?.Invoke(peerId, playerInfo);
        Logger.Info("玩家加入: [{peerId}] {name}", peerId, name);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcRemovePlayer(long peerId)
    {
        _players.Remove(peerId);
        PlayerRemoved?.Invoke(peerId);
        Logger.Info("玩家离开: [{peerId}]", peerId);
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RpcStartGame()
    {
        GameStart?.Invoke();
    }
}