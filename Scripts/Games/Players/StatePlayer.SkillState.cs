using System.Collections.Generic;
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
        private Vector2I[]? _currentSkillArgs;
        private IReadOnlyUnitState? _unitState;
        private HashSet<Vector2I> _validSkillCoords = new();
        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId);
            _validSkills = GameState?.GetUnitValidSkills("defalut") ?? [];
            TryBeginNewSkill(0);
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
            _currentArgIndex = 0;
            _currentSkillArgs = new Vector2I[_currentSkill.ArgCount];

            GameScheduler?.AddEvent(_currentSkillEvent);
            UpdateValidSkillCoords();
        }

        private void SetCurrentIndexArg(in Vector2I arg)
        {
            if (_currentSkillArgs is null || _currentSkill is null) return;
            _currentSkillArgs[_currentArgIndex] = arg;
            _currentSkillEvent?.Args =  _currentSkillArgs;
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
                _currentArgIndex--;
                UpdateValidSkillCoords();
            }
        }
        private void UpdateValidSkillCoords()
        {
            if (_unitState is null || _currentSkill is null || _currentSkillArgs is null) return;
            _validSkillCoords.Clear();
            var validArgs = _currentSkill.GetSkillArgs(_unitState.Coords, _currentArgIndex, _currentSkillArgs);
            foreach (var coord in validArgs)
            {
                _validSkillCoords.Add(coord);
            }
        }
        private bool IsCurrentValidArg()
        {
            return _currentSkillArgs is not null && _validSkillCoords.Contains(_currentSkillArgs[_currentArgIndex]);
        }
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