using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class FreeState(StatePlayer player) : State(player)
    {
        private SkillEvent? _skillEvent;
        public uint FocusedUnitId
        {
            get => field;
            set
            {
                if (value == field) return;
                _skillEvent?.Layer = SimulationLayer.Waiting;
                field = value;
                _skillEvent = GameScheduler?.GetUnitSkillEvent(value);
                _skillEvent?.Layer = SimulationLayer.Focused;
            }
        } = 0;

        public override void OnEnter()
        {
            FocusCoords(_player.CurrentMouseCoords);
        }
        public override void OnInput<T>(in T input)
        {
            if (!_player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click:
                    if (GameState?.UnitMapState.HasUnit(FocusedUnitId) == false) break;
                    var skill = GameScheduler?.GetUnitSkillEvent(FocusedUnitId);
                    if (skill is null)
                    {
                        //单位没有等待中的技能
                        StateMachine.ChangeState(_player._skillingState);
                    }
                    break;
                case PlayerInputMouseCoords coords:
                    FocusCoords(coords.Coords);
                    break;
                case PlayerInputStopTurn stopTurn:
                    if (stopTurn.Type == PlayerSotpTurnEnum.Click)
                    {
                        _player.TurnManager.FocusTurnEvent(-1);
                        _player.IsTurn = false;
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverExit)
                    {
                        _player.TurnManager.FocusTurnEvent(-1);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverEnter)
                    {
                        FocusedUnitId = 0;
                        _player.TurnManager.FocusTurnEvent(_player.TurnManager.CurrentTurn + 1);
                    }
                    break;
            }
        }

        private void FocusCoords(in Vector2I coords)
        {
            var unit = GameScheduler?.GameState?.UnitMapState.GetUnit(coords);
            FocusedUnitId = unit?.Id ?? 0;
            GameScheduler?.FocusUnit(unit?.Id ?? 0);
        }
    }
}