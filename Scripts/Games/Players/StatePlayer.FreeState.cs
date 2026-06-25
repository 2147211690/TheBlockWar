using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class FreeState(StatePlayer player) : State(player)
    {
        public long FocusedUnitId
        {
            get => Player.FocusedUnitId;
            set
            {
                if (value == Player.FocusedUnitId) return;
                EventScheduler?.GetUnitSkillEvent(Player.FocusedUnitId)?.Layer = SimulationLayer.Waiting;
                Player.GameView.GetUnitView(FocusedUnitId)?.IsFocused = false;
                Player.FocusedUnitId = value;
                EventScheduler?.GetUnitSkillEvent(Player.FocusedUnitId)?.Layer = SimulationLayer.Focused;
                Player.GameView.GetUnitView(FocusedUnitId)?.IsFocused = true;
                
            }
        }
        public override void OnEnter()
        {
            Player.SelectedUnitId = 0;
            FocusCoords(Player.CurrentMouseCoords);
        }
        public override void OnInput<T>(in T input)
        {
            if (!Player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click:
                    var u = EventScheduler?.GameState.UnitMapState.GetUnit(FocusedUnitId);
                    if (u.IsNull()) break;
                    Player.SelectedUnitId = FocusedUnitId;
                    var skill = EventScheduler?.GetUnitSkillEvent(FocusedUnitId);
                    if (skill == null && u.Team == Player.Team)
                    {
                        //单位没有等待中的技能
                        Player._skillState.CurrentSkillIndex = 0;
                        StateMachine.ChangeState(Player._skillState);
                    }
                    else
                    {
                        StateMachine.ChangeState(Player._selectState);
                    }
                    break;
                case PlayerInputMouseCoords coords:
                    FocusCoords(coords.Coords);
                    break;
                case PlayerInputStopTurn stopTurn:
                    if (stopTurn.Type == PlayerSotpTurnEnum.Click)
                    {
                        Player.TurnManager.FocusNextTurn(-1);
                        FocusedUnitId = 0;
                        
                        Player._busyState.CallOperation = new PlayerEndTurn(Player.Id);
                        Player._busyState.NextState = Player._freeState;
                        StateMachine.ChangeState(Player._busyState);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverExit)
                    {
                        Player.TurnManager.FocusNextTurn(-1);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverEnter)
                    {
                        Player.TurnManager.FocusNextTurn(Player.TurnManager.CurrentTurn + 1);
                    }
                    break;
            }
        }

        private void FocusCoords(in Vector2I coords)
        {
            var unit = EventScheduler?.GameState.UnitMapState.GetUnit(coords);
            FocusedUnitId = unit?.Id ?? 0;
        }
    }
}