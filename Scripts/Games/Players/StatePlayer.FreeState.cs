using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class FreeState(StatePlayer player) : State(player)
    {
        private SkillSchedulerEvent? _skillEvent;
        public IReadOnlyUnitState SelectedUnit { get; set; }
        public long FocusedUnitId
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
        public long SelectedUnitId
        {
            get => _player.SelectedUnitId;
            set
            {
                if (value == _player.SelectedUnitId) return;
                SelectedUnit = GameState?.UnitMapState.GetUnit(value)!;
                var skills = GameState.GetUnitValidSkills(SelectedUnit.Name);
                player._skillStates = new SkillState[skills.Count];
                for (int i = 0; i < skills.Count; i++)
                {
                    player._skillStates[i] = new SkillState()
                    {
                        Skill = skills[i],
                        IsSelecting = false,
                        RemainingUses = 1, //TODO:使用Info加载剩余次数
                    };
                }
                _player.OnPropertyChanged(nameof(SkillingUnitSkillStates));
                _player.SelectedUnitId = value;
            }
        }
        public override void OnEnter()
        {
            FocusCoords(_player.CurrentMouseCoords);
            SelectedUnitId = 0;
        }
        public override void OnInput<T>(in T input)
        {
            if (!_player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click:
                    if (GameState?.UnitMapState.HasUnit(FocusedUnitId) == false) break;
                    SelectedUnitId = FocusedUnitId;
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