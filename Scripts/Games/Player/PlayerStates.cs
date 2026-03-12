using Godot;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Player;
public partial class StatePlayer
{
    private abstract class State(StatePlayer player) : IState
    {
        protected StatePlayer _player = player;
        protected Logger Logger => StatePlayer.Logger;
        public IReadOnlyGameState? GameState => _player.GameScheduler?.GameState;
        public GameScheduler? GameScheduler => _player.GameScheduler;
        public uint FocusedUnitId => _player.GameScheduler?.GameState?.FocusedUnitId ?? 0;
        public virtual void OnEnter() {}
        public virtual void OnExit() {}
        public virtual void OnInput<T>(in T input) {}
    }

    private class FreeState(StatePlayer player) : State(player)
    {
        public Vector2I? PreviousCoords { get; set; }
        public Vector2I CurrentCoords { get; set; }

        public override void OnEnter()
        {
            PreviousCoords = null;
        }
        public override void OnInput<T>(in T input)
        {
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick click:
                    if (GameScheduler == null || GameState?.HasUnit(GameState.FocusedUnitId) == false) break;
                    var skill = GameScheduler?.GetUnitSkillEvent(FocusedUnitId);
                    if (skill == null)
                    {
                        //单位没有等待中的技能
                    }
                    else
                    {
                        //单位有等待中的技能
                        skill.Run();
                    }
                    break;
                case PlayerInputMousceCoords coords:
                    if (PreviousCoords == null || coords.Coords != PreviousCoords)
                    {
                        PreviousCoords = CurrentCoords;
                        CurrentCoords = coords.Coords;
                        
                        //坐标改变后
                        GameScheduler?.FocusCoords(CurrentCoords);
                    }
                    break;
            }
        }
    }
}