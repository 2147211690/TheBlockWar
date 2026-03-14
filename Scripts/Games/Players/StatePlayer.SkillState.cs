using System.Collections.Generic;
using System.Linq;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class SkillState(StatePlayer player) : State(player)
    {
        private SkillEvent? _currentSkillEvent;
        private IReadOnlyList<ISkill> _validSkills = [];
        private ISkill? _currentSkill;
        private int _currentArgIndex = 0;
        private Vector2I?[]? _currentSkillArgs;
        private IReadOnlyUnitState? _unitState;
        private uint _validRegionId;
        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId);
            _validSkills = GameState?.GetUnitValidSkills("defalut") ?? [];
            TryBeginNewSkill(0);
        }

        public override void OnExit()
        {
            base.OnExit();
            GameScheduler?.RemoveCoordsRegion(_validRegionId);
            _validSkills = [];
            _currentSkill = null;
            _currentSkillEvent = null;
            _unitState = null;
            _currentSkillArgs = null;
            _validRegionId = 0;
        }

        public override void OnInput<T>(in T input)
        {
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click: //选定
                    if (IsCurrentValidArg())
                    {
                        if (IsEndArg())
                        {
                            _currentSkillEvent?.Run();
                            StateMachine.ChangeState(player._freeState);
                        }
                        else
                        {
                            EnterNextArg();
                        }
                    }
                    break;
                case PlayerInputClick {Button: MouseButtonMask.Right} click: //取消
                    if (IsFirstArg())
                    {
                        GameScheduler?.RemoveEvent(_currentSkillEvent!);
                        StateMachine.ChangeState(player._freeState);
                    }
                    else
                    {
                        BackToPreviousArg();
                    }
                    break;
                case PlayerInputMousceCoords coords:
                    SetCurrentIndexArg(coords.Coords - _unitState?.Coords ?? Vector2I.Zero);
                    break;
            }
        }

        private void TryBeginNewSkill(int index)
        {
            if (index < 0 || index >= _validSkills.Count) return;
             _currentSkill = _validSkills[index];
             if (_currentSkill.ArgCount == 0)
             {
                 StateMachine.ChangeState(player._freeState);
                 return;
             }
            _currentSkillEvent = EventFactory.CreateSkill(FocusedUnitId, _currentSkill);
            _currentSkillEvent.Team = _unitState!.Team;
            _currentArgIndex = 0;
            _currentSkillArgs = new Vector2I?[_currentSkill.ArgCount];

            GameScheduler?.AddEvent(_currentSkillEvent);
            UpdateValidSkillCoords();
        }

        private void SetCurrentIndexArg(in Vector2I arg)
        {
            if (_currentSkillArgs is null || _currentSkill is null) return;
            _currentSkillArgs[_currentArgIndex] = 
                IsArgValid(arg + _unitState?.Coords ?? Vector2I.Zero) ? arg : null;
            _currentSkillEvent?.Args = _currentSkillArgs;
        }

        private void EnterNextArg()
        {
            if (_currentSkill is null) return;
            if (_currentArgIndex < _currentSkill.ArgCount - 1)
            {
                _currentArgIndex++;
                UpdateValidSkillCoords();
            }
        }
        private void BackToPreviousArg()
        {
            if (_currentArgIndex > 0)
            {
                _currentSkillArgs![_currentArgIndex] = null;
                _currentArgIndex--;
                UpdateValidSkillCoords();
                _currentSkillArgs![_currentArgIndex] = 
                    IsArgValid(_player.CurrentMouseCoords) ?
                        _player.CurrentMouseCoords - _unitState!.Coords : null;
                _currentSkillEvent?.Args = _currentSkillArgs;
            }
        }
        private void UpdateValidSkillCoords()
        {
            if (_unitState is null || _currentSkill is null || _currentSkillArgs is null) return;
            GameScheduler?.RemoveCoordsRegion(_validRegionId);
            var validArgs =
                _currentSkill.GetSkillArgs(GameState!, _unitState, _currentArgIndex, _currentSkillArgs).
                    Select(a => a + _unitState.Coords);
            _validRegionId = GameScheduler?.AddCoordsRegion(validArgs, 0) ?? 0;
        }
        private bool IsCurrentValidArg()
        {
            return _currentSkillArgs is not null &&
                   GameScheduler?.IsInCoordsRegion(_validRegionId, 
                       _currentSkillArgs[_currentArgIndex] + _unitState?.Coords ?? Vector2I.Zero) == true;
        }
        private bool IsArgValid(Vector2I coords) =>  _currentSkillArgs is not null &&
                   GameScheduler?.IsInCoordsRegion(_validRegionId, coords) == true;
        private bool IsEndArg()
        {
            return _currentArgIndex == _currentSkill!.ArgCount - 1;
        }
        private bool IsFirstArg()
        {
            return _currentArgIndex == 0;
        }
    }
}