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
        private bool _lockTileCoords = false;
        private IReadOnlyList<EventState>? unitEvents;
        public long FocusedUnitId
        {
            get => Player.FocusedUnitId;
            set
            {
                if (value == Player.FocusedUnitId) return;
                unitEvents?.SetLayer(SimulationLayer.Waiting);
                Player.GameView.GetUnitView(FocusedUnitId)?.IsFocused = false;
                Player.FocusedUnitId = value;
                unitEvents = EventScheduler?.GetUnitEvents(value);
                unitEvents?.SetLayer(SimulationLayer.Focused);
                Player.GameView.GetUnitView(FocusedUnitId)?.IsFocused = true;
                
            }
        }
        public override void OnEnter()
        {
            Player.SelectedUnitId = 0;
            _lockTileCoords = false;
            FocusCoords(Player.CurrentMouseCoords);
        }
        public override void OnInput<T>(in T input)
        {
            if (!Player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick click:
                    if (click.Type == PlayerClickTypeEnum.Left)
                    {
                        var u = EventScheduler?.GameState.UnitMapState.GetUnit(FocusedUnitId);
                        if (u.IsNull()) break;
                        Player.SelectedUnitId = FocusedUnitId;
                        unitEvents = EventScheduler?.GetUnitEvents(FocusedUnitId);
                        if (unitEvents.Count == 0 && u.Team == Player.Team)
                        {
                            //单位没有等待中的技能
                            Player._skillState.CurrentSkillIndex = 0;
                            FocusEventCoords = null;
                            StateMachine.ChangeState(Player._skillState);
                        }
                        else
                        {
                            StateMachine.ChangeState(Player._selectState);
                        }
                    }
                    else if (click.Type == PlayerClickTypeEnum.Right)
                    {
                        _lockTileCoords = !_lockTileCoords;
                        if (!_lockTileCoords) Player.SelectedTileCoords = Player.CurrentMouseCoords;
                    }
                    break;
                case PlayerInputMouseCoords coords:
                    FocusCoords(coords.Coords);
                    break;
                case PlayerInputStopTurn stopTurn:
                    if (stopTurn.Type == PlayerSotpTurnEnum.Click)
                    {
                        Player.TurnManager.FocusNextTurn(false);
                        FocusedUnitId = 0;
                        
                        Player._busyState.CallOperation = new PlayerEndTurn(Player.Id);
                        Player._busyState.NextState = Player._freeState;
                        FocusEventCoords = null;
                        StateMachine.ChangeState(Player._busyState);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverExit)
                    {
                        Player.TurnManager.FocusNextTurn(false);
                    }
                    else if (stopTurn.Type == PlayerSotpTurnEnum.HoverEnter)
                    {
                        Player.TurnManager.FocusNextTurn(true);
                    }
                    break;
            }
        }

        private void FocusCoords(in Vector2I coords)
        {
            var unit = EventScheduler?.GameState.UnitMapState.GetUnit(coords);
            FocusedUnitId = unit?.Id ?? 0;
            FocusEventCoords = coords;
            if (!_lockTileCoords) Player.SelectedTileCoords = coords;
        }
    }
}