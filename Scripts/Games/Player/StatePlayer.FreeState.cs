using Godot;

namespace TheBlockWar.Scripts.Games.Player;

public partial class StatePlayer
{
    private class FreeState(StatePlayer player) : State(player)
    {
        public override void OnEnter()
        {
        }
        public override void OnInput<T>(in T input)
        {
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick click:
                    if (GameState?.UnitMapState.HasUnit(FocusedUnitId) == false) break;
                    var skill = GameScheduler?.GetUnitSkillEvent(FocusedUnitId);
                    if (skill == null)
                    {
                        //单位没有等待中的技能
                        StateMachine.ChangeState(_player._skillState);
                    }
                    else
                    {
                        //单位有等待中的技能
                        skill.Run();
                    }
                    break;
                case PlayerInputMousceCoords coords:
                        GameScheduler?.FocusCoords(coords.Coords);
                    break;
            }
        }
    }
}