using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games;

public interface ISkill
{
    int ArgCount { get; }
    /// <summary>
    /// 获取技能的参数
    /// </summary>
    /// <param name="unitCoords">单位坐标</param>
    /// <param name="argsIndex">参数索引,用来指定当前可选参数的位置</param>
    /// <param name="args">参数列表</param>
    /// <returns>当前有效参数列表</returns>
    IEnumerable<Vector2I> GetSkillArgs(Vector2I unitCoords, int argsIndex, Vector2I[] args);
    void SkillCore(IGameState gameState, SimulationResult result, IReadOnlyUnitState unit, Vector2I[] args);
}