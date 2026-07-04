using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Onlines;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class SelectState(StatePlayer player) : State(player)
    {
        IReadOnlyUnitState _selectedUnit = UnitState.Null;

        public override void OnEnter()
        {
            _selectedUnit = EventScheduler?.GameState.UnitMapState.GetUnit(Player.SelectedUnitId) ?? UnitState.Null;
            var skills = EventScheduler.GameState.GetUnitValidSkills(_selectedUnit.Name);
            if (skills.Count == 0) return;
            var s = skills[0];
            Player.GameView.ShowRegion(
                s.GetSkillArgs(EventScheduler.GameState, _selectedUnit, _selectedUnit.Coords, 0, [])
                    .Select(a => a + _selectedUnit.Coords),
                _selectedUnit.Team, RegionShowTypeEnum.Preview);
            Player.SelectedTileCoords = Player.CurrentMouseCoords;
        }

        public override void OnExit()
        {
            Player.GameView.ClearRegion();
        }

        public override void OnInput<T>(in T input)
        {
            if (!Player.IsTurn) return;
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick click:
                    StateMachine.ChangeState(Player._freeState);
                    if (click.Type == PlayerClickTypeEnum.Left)
                    {
                        Player._freeState.OnInput(click);
                    }
                    break;
                case PlayerInputMouseCoords coords:
                    Player.SelectedTileCoords = coords.Coords;
                    FocusEventCoords = coords.Coords;
                    break;
                case PlayerInputSkill skill:
                    if (_selectedUnit.Team != Player.Team) break;
                    Player._skillState.CurrentSkillIndex = skill.Index;
                    FocusEventCoords = null;
                    StateMachine.ChangeState(Player._skillState);
                    break;
                case PlayerInputStopTurn stopTurn:
                    if (stopTurn.Type == PlayerSotpTurnEnum.Click)
                    {
                        StateMachine.ChangeState(Player._freeState);
                        Player._freeState.OnInput(stopTurn);
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
    }
}