using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class FreeState(StatePlayer player) : State(player)
    {
        public IReadOnlyUnitState SelectedUnit { get; set; } = UnitState.Null;
        public new long FocusedUnitId
        {
            get => Player.FocusedUnitId;
            set
            {
                if (value == Player.FocusedUnitId) return;
                EventScheduler?.GetUnitSkillEvent(Player.FocusedUnitId)?.Layer = SimulationLayer.Waiting;
                Player.FocusedUnitId = value;
                EventScheduler?.GetUnitSkillEvent(Player.FocusedUnitId)?.Layer = SimulationLayer.Focused;
            }
        }
        public long SelectedUnitId
        {
            get => Player.SelectedUnitId;
            set
            {
                if (value == Player.SelectedUnitId) return;
                SelectedUnit = GameState?.UnitMapState.GetUnit(value)!;
                var skills = GameState?.GetUnitValidSkills(SelectedUnit.Name) ?? [];
                Player._skillStates = new SkillState[skills.Count];
                for (int i = 0; i < skills.Count; i++)
                {
                    Player._skillStates[i] = new SkillState()
                    {
                        Skill = skills[i],
                        IsSelecting = false,
                        RemainingUses = 1, //TODO:使用Info加载剩余次数
                    };
                }
                Player.OnPropertyChanged(nameof(SkillingUnitSkillStates));
                Player.SelectedUnitId = value;
            }
        }
        public override void OnEnter()
        {
            FocusCoords(Player.CurrentMouseCoords);
            SelectedUnitId = 0;
        }
        public override void OnInput<T>(in T input)
        {
            if (!Player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click:
                    if (GameState?.UnitMapState.HasUnit(FocusedUnitId) == false) break;
                    SelectedUnitId = FocusedUnitId;
                    var skill = EventScheduler?.GetUnitSkillEvent(FocusedUnitId);
                    if (skill is null)
                    {
                        //单位没有等待中的技能
                        Player._skillingState.CurrentSkillIndex = 0;
                        StateMachine.ChangeState(Player._skillingState);
                    }
                    break;
                case PlayerInputMouseCoords coords:
                    FocusCoords(coords.Coords);
                    break;
                case PlayerInputStopTurn stopTurn:
                    if (stopTurn.Type == PlayerSotpTurnEnum.Click)
                    {
                        Player.TurnManager.FocusTurnEvent(-1);
                        Player.IsTurn = false;
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverExit)
                    {
                        Player.TurnManager.FocusTurnEvent(-1);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverEnter)
                    {
                        FocusedUnitId = 0;
                        Player.TurnManager.FocusTurnEvent(Player.TurnManager.CurrentTurn + 1);
                    }
                    break;
            }
        }

        private void FocusCoords(in Vector2I coords)
        {
            var unit = EventScheduler?.GameState?.UnitMapState.GetUnit(coords);
            FocusedUnitId = unit?.Id ?? 0;
            EventScheduler?.FocusUnit(unit?.Id ?? 0);
        }
    }
}