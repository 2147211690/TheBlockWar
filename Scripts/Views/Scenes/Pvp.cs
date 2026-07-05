using System.Linq;
using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Players;
using TheBlockWar.Scripts.Games.Turns;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views.Uis;

namespace TheBlockWar.Scripts.Views.Scenes;
public partial class Pvp : Node2D
{
    [ExportGroup("Self")]
    [Export] private GameView _gameView = null!;
    [Export] private CombatUi _combatUi = null!;
    private IGameState? _gameState;
    private IEventScheduler? _eventScheduler;
    private ITurnStrategy? _pvpMode;
    private ITurnManager? _turnManager;
    public void StartGame((long Id, PlayerInfo PlayerInfo)[] playerInfos, int aiCount, long localId, MultiplayerPeer? peer = null)
    {
        TaskTracker.ShowTrackerWindow();
        var localIndex = Array.FindIndex(playerInfos, p => p.Id == localId) + aiCount;
        
        _gameState = new GameState(
            new Vector2I(10, 10),
            TeamRelationTool.GetAllTeamEnemy(playerInfos.Length + aiCount)
        );
        _eventScheduler = new EventScheduler(_gameState, localIndex);
        
        _gameView.Init(_eventScheduler);
        _gameView.PlayerTeam = localIndex;
        
        var playerOperationSynchronizer = new OnlinePlayerOperationSynchronizer();
        AddChild(playerOperationSynchronizer);
        playerOperationSynchronizer.Multiplayer.MultiplayerPeer = peer;
        
        var players = new IPlayer[playerInfos.Length + aiCount];
        for (int i = 0; i < playerInfos.Length + aiCount; i++)
        {
            if (i < aiCount)
            {
                players[i] = new EasyAiPlayer(100, _eventScheduler, playerOperationSynchronizer);
            }
            else if (i == localIndex)
            {
                var sp = StatePlayer.Create(playerInfos[i - aiCount].Id, _gameView, _eventScheduler, playerOperationSynchronizer);
                AddChild(sp);
                players[i] = sp;
            }
            else
            {
                players[i] = new FakePlayer(playerInfos[i - aiCount].Id);
            }
        }
            
        _pvpMode = new PvpTurnStrategy(
            playerOperationSynchronizer, 
            players,
            new RandomMapBuilder(
                [TileNames.Soil0, TileNames.水块, TileNames.燃烧块], 
                [UnitNames.Unit0, UnitNames.Unit1, UnitNames.Unit2],
                Enumerable.Range(0, players.Length).Select((p, i) => (i, 8)).ToArray(),
                1
                )
        );
        _turnManager = new TurnManager(_eventScheduler, _pvpMode);
        
        _combatUi.Bind(_eventScheduler, _turnManager, _gameView, players[localIndex]);
        
        _turnManager.Start().Forget();
    }
}