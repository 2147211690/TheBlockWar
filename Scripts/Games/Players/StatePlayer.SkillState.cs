using System.Linq;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class SkillingState(StatePlayer player) : State(player)
    {
        public SkillState[] SkillStates = [];
        private SkillEvent? _currentSkillEvent;
        private IReadOnlyList<ISkill> _validSkills = [];
        private ISkill _currentSkill;
        private int _currentArgIndex = -1;
        private Vector2I?[]? _currentSkillArgs;
        private IReadOnlyUnitState? _unitState;
        private uint _validRegionId;
        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId);
            _validSkills = GameState?.GetUnitValidSkills("defalut") ?? [];
            SkillStates = new SkillState[_validSkills.Count];
            for (int i = 0; i < _validSkills.Count; i++)
            {
                SkillStates[i] = new SkillState()
                {
                    Skill = _validSkills[i],
                    IsSelecting = false,
                    RemainingUses = 1, //TODO:使用Info加载剩余次数
                };
            }
            //从这里面通知SkillStates改变
            TryBeginNewSkill(0);
        }

        public override void OnExit()
        {
            base.OnExit();
            //GameScheduler!.RemoveCoordsRegion(_validRegionId);
            _validSkills = [];
            SkillStates = [];
            _currentSkill = null;
            _currentSkillEvent = null;
            _unitState = null;
            _currentSkillArgs = null;
            _validRegionId = 0;
            _currentArgIndex = -1;
            NotifySkillStatesChanged();
        }

        public override void OnInput<T>(in T input)
        {
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick {Button: MouseButtonMask.Left} click: //选定
                    TryEnterToNextArg();
                    break;
                case PlayerInputClick {Button: MouseButtonMask.Right} click: //取消
                    TryBackToPreviousArg();
                    break;
                case PlayerInputMouseCoords coords:
                    SetCurrentIndexArgGlobal(coords.Coords);
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

             for (var i = 0; i < SkillStates.Length; i++) SkillStates[i].IsSelecting = false;
             SkillStates[index].IsSelecting = true;
             NotifySkillStatesChanged();
             TryEnterToNextArg();
        }



        private void TryEnterToNextArg()
        {
            if (_currentArgIndex == -1) //进入第一个参数
            {
                _currentSkillEvent = EventFactory.CreateSkill(FocusedUnitId, _currentSkill);
                _currentSkillEvent.Team = _unitState!.Team;
                _currentSkillArgs = new Vector2I?[_currentSkill.ArgCount];
                _currentSkillEvent.Args = _currentSkillArgs;
                
                _currentArgIndex++;
                
                UpdateCurrentValidSkillCoords();
                SetCurrentIndexArgGlobal(_player.CurrentMouseCoords);
                GameScheduler?.AddEvent(_currentSkillEvent);
            }
            else if (_currentArgIndex == _currentSkill.ArgCount - 1) //最后一个参数
            {
                if (!IsCurrentValidArg()) return;
                //所有参数完成
                GameScheduler!.RemoveCoordsRegion(_validRegionId);
                //_currentSkillEvent!.Run();
                StateMachine.ChangeState(player._freeState);
            }
            else //进入下一个参数
            {
                if (!IsCurrentValidArg()) return;
                _currentArgIndex++;
                UpdateCurrentValidSkillCoords();
                SetCurrentIndexArgGlobal(_player.CurrentMouseCoords);
            }
        }
        private void TryBackToPreviousArg()
        {
            if (_currentArgIndex == 0) //如果已经在第一个参数，取消技能
            {
                GameScheduler!.RemoveCoordsRegion(_validRegionId);
                GameScheduler.RemoveEvent(_currentSkillEvent!);
                _currentArgIndex--;
                StateMachine.ChangeState(player._freeState);
            }
            else if (_currentArgIndex < 0) //未初始状态
            {
                return;
            }
            else //返回上一个参数
            {
                _currentSkillArgs![_currentArgIndex] = null;
                _currentArgIndex--;
                UpdateCurrentValidSkillCoords();
                SetCurrentIndexArgGlobal(_player.CurrentMouseCoords);
            }
        }
        
        private void SetCurrentIndexArgGlobal(in Vector2I arg)
        {
            _currentSkillArgs![_currentArgIndex] = IsArgGlobalValid(arg) ? arg - _unitState!.Coords: null;
            _currentSkillEvent?.Args = _currentSkillArgs;
        }
        private void UpdateCurrentValidSkillCoords()
        {
            GameScheduler?.RemoveCoordsRegion(_validRegionId);
            var validArgs =
                _currentSkill.GetSkillArgs(GameState!, _unitState!, _unitState!.Coords, _currentArgIndex, _currentSkillArgs!).
                    Select(a => a + _unitState!.Coords);
            _validRegionId = GameScheduler?.AddCoordsRegion(validArgs, 0) ?? 0;
        }
        private bool IsCurrentValidArg()
        {
            return _currentSkillArgs![_currentArgIndex] is not null &&
                GameScheduler!.IsInCoordsRegion(_validRegionId, 
                       _currentSkillArgs![_currentArgIndex]!.Value + _unitState!.Coords);
        }
        private bool IsArgGlobalValid(Vector2I? coords) =>  coords is not null && 
                                                            GameScheduler?.IsInCoordsRegion(_validRegionId, coords.Value) == true;
        private void NotifySkillStatesChanged()
        {
            _player.OnPropertyChanged(nameof(_player.SkillingUnitSkillStates));
        }
    }
}