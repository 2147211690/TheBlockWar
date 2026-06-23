using TheBlockWar.Scripts.Games;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Views.TileViews;
using TheBlockWar.Scripts.Views.UnitViews;
using TheBlockWar.Scripts.Views.ViewNodes;

namespace TheBlockWar.Scripts.Views;
/// <summary>
/// 游戏视图接口
/// 所有Event开头的函数用于事件绘制,在下一次绘制到来时会被清理
/// </summary>
public interface IGameView
{
    ViewPools Pools { get; }
    bool IsRunning { get; }
    #region Data
    IReadOnlyGameState GameState { get; }
    IEventScheduler Scheduler { get; }
    Node2D DrawContainer { get; }
    #endregion
    
    #region Style
    GameViewStyle GameStyle { get; }
    GameViewTeamStyle CurrentStyle { get; }
    #endregion
    
    #region Team
    int PlayerTeam { get; set; }
    TeamTypeEnum GetTeamType(int team);
    TeamTypeEnum CurrentTeamType { get; }
    #endregion
    
    #region Position
    Vector2 NodeRight { get; }
    Vector2 NodeLeft { get; }
    Vector2 NodeUp { get; }
    Vector2 NodeDown { get; }
    float NormalGridHeight { get; }
    
    /// <summary>
    /// 获取游戏坐标对应的3D位置
    /// </summary>
    /// <param name="coords">游戏坐标</param>
    /// <param name="heightOffset">高度偏移量</param>
    /// <param name="height">高度,如果空则使用地面高度</param>
    /// <returns></returns>
    Vector3 CoordsToGamePosition(Vector2 coords, float heightOffset = 0, float? height = null);
    Vector2 GamePositionToNodePosition(in Vector3 gamePosition);
    Vector2I NodePositionToCoords(Vector2 nodePosition);
    //Vector2 GamePositionToNodeLocalPosition(Vector3 gamePosition);
    #endregion
    
    #region Unit
    UnitView? CreateUnitView(Vector2I coords, IReadOnlyUnitState unitState);
    UnitView? GetUnitView(long id);
    UnitView? GetUnitView(Vector2I coords);
    void FreeUnitView(long id);
    void SetUnitRealCoords(long id, Vector2I coords);
    #endregion
    
    #region Tile
    TileView? SetTileView(Vector2I coords, string tileName);
    TileView? GetTileView(Vector2I coords);
    float GetTileHeight(Vector2I coords);
    TileInfo GetTileInfo(Vector2I coords);
    #endregion
    
    #region Event
    /// <summary>
    /// 获取单位视图并标记事件清理事件绘制
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    UnitView? EventUnit(long id);
    ViewSprite2D EventSprite();
    void EventTopDraw(Action<ViewNode2D> action, int zOffset = 0);
    ViewNode2D EventDraw(Action<ViewNode2D> action);
    void EventHealth(Vector2I coords, float originHealthChange ,float validHealthChange, bool hasTarget , bool isFocused);
    void EventBuff(Vector2I coords, string buffName, int countChange, bool isFocused);
    void EventRegion(Vector2I coords, string regionName, bool isAdd, bool isFocused);
    
    #endregion

    ViewNode2D? CreateEffect(string effectName);

    #region Region
    void ShowRegion(IEnumerable<Vector2I> coords, int team, RegionShowTypeEnum showTypeEnum);
    void ClearRegion();
    #endregion
}
public enum RegionShowTypeEnum
{
    Skill,
}