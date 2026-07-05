using System.Text.Json.Serialization;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Events;

[JsonDerivedType(typeof(SkillEventCore), "SkillEventCore")]
[JsonDerivedType(typeof(StepsEventCore), "StepsEventCore")]
public interface IEventCore
{
    /// <summary>
    /// 获取事件绑定的单位id,没有则为0
    /// </summary>
    /// <param name="eventState"></param>
    /// <returns></returns>
    long GetBindUnitId(EventState eventState);
    Vector2I? GetCoords(EventState eventState);
    void OnApply(EventState eventState, SimObject obj);
    bool CheckIsValid(EventState eventState, IReadOnlyGameState gameState);
}