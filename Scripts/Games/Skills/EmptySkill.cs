using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Skills;

public class EmptySkill(string skillName) : Skill(skillName, 0)
{
    public override IEnumerable<Vector2I> GetSkillArgs(IReadOnlyGameState state, IReadOnlyUnitState caster,
        Vector2I useCoords, int argsIndex,
        Vector2I?[] args) => [];

    protected override void Core(SimObject obj, IReadOnlyUnitState unit,
        Vector2I useCoords,
        Vector2I?[] args){}
}