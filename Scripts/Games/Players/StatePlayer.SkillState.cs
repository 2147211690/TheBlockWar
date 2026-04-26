using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class SkillingState(StatePlayer player) : State(player)
    {
        private SkillEvent? _currentSkillEvent;
        private IReadOnlyList<ISkill> _validSkills = [];
        private ISkill _currentSkill = null!;
        private int _currentArgIndex = -1;
        private Vector2I?[]? _currentSkillArgs;
        private IReadOnlyUnitState _unitState = UnitState.Null;
        private long _validRegionId;

        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId) ?? UnitState.Null;
            _validSkills = GameState?.GetUnitValidSkills(_unitState.Name) ?? [];
            //从这里面通知SkillStates改变
            TryBeginNewSkill(0);
        }

        public override void OnExit()
        {
            base.OnExit();
            //GameScheduler!.RemoveCoordsRegion(_validRegionId);
            _validSkills = [];
            _currentSkill = null!;
            _currentSkillEvent = null;
            _unitState = UnitState.Null;
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
                case PlayerInputClick { Button: MouseButtonMask.Left } click: //选定
                    TryEnterToNextArg();
                    break;
                case PlayerInputClick { Button: MouseButtonMask.Right } click: //取消
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
                StateMachine.ChangeState(Player._freeState);
                return;
            }

            for (var i = 0; i < Player._skillStates.Length; i++) Player._skillStates[i].IsSelecting = false;
            Player._skillStates[index].IsSelecting = true;
            NotifySkillStatesChanged();
            TryEnterToNextArg();
        }


        private void TryEnterToNextArg()
        {
            if (_currentArgIndex == -1) //进入第一个参数
            {
                _currentSkillEvent = EventScheduler!.CreateEvent(id => new SkillEvent(id)
                {
                    SkillBindUnitId = Player.SelectedUnitId,
                    SkillEnum = SkillEnum.Test1,
                    Team = _unitState.Team,
                });

                _currentSkillArgs = new Vector2I?[_currentSkill.ArgCount];
                _currentSkillEvent.SkillArgs = _currentSkillArgs;
                _currentSkillEvent.Layer = SimulationLayer.Focused;
                _currentArgIndex++;

                UpdateCurrentValidSkillCoords();
                SetCurrentIndexArgGlobal(Player.CurrentMouseCoords);
            }
            else if (_currentArgIndex == _currentSkill.ArgCount - 1) //最后一个参数
            {
                if (!IsCurrentValidArg()) return;
                //所有参数完成
                EventScheduler!.RemoveCoordsRegion(_validRegionId);
                _currentSkillEvent!.Turn = Player.TurnManager.CurrentTurn + 1;
                StateMachine.ChangeState(Player._freeState);
            }
            else //进入下一个参数
            {
                if (!IsCurrentValidArg()) return;
                _currentArgIndex++;
                UpdateCurrentValidSkillCoords();
                SetCurrentIndexArgGlobal(Player.CurrentMouseCoords);
            }
        }

        private void TryBackToPreviousArg()
        {
            if (_currentArgIndex == 0) //如果已经在第一个参数，取消技能
            {
                EventScheduler!.RemoveCoordsRegion(_validRegionId);
                EventScheduler.DeleteEvent(_currentSkillEvent!);
                _currentArgIndex--;
                StateMachine.ChangeState(Player._freeState);
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
                SetCurrentIndexArgGlobal(Player.CurrentMouseCoords);
            }
        }

        private void SetCurrentIndexArgGlobal(in Vector2I arg)
        {
            _currentSkillArgs![_currentArgIndex] = IsArgGlobalValid(arg) ? arg - _unitState.Coords : null;
            _currentSkillEvent?.SkillArgs = _currentSkillArgs;
        }

        private void UpdateCurrentValidSkillCoords()
        {
            EventScheduler?.RemoveCoordsRegion(_validRegionId);
            var validArgs =
                _currentSkill
                    .GetSkillArgs(GameState!, _unitState, _unitState.Coords, _currentArgIndex, _currentSkillArgs!)
                    .Select(a => a + _unitState.Coords);
            _validRegionId = EventScheduler?.AddCoordsRegion(validArgs, 0) ?? 0;
        }

        private bool IsCurrentValidArg()
        {
            return _currentSkillArgs![_currentArgIndex] is not null &&
                   EventScheduler!.IsInCoordsRegion(_validRegionId,
                       _currentSkillArgs![_currentArgIndex]!.Value + _unitState.Coords);
        }

        private bool IsArgGlobalValid(Vector2I? coords) => coords is not null &&
                                                           EventScheduler?.IsInCoordsRegion(_validRegionId,
                                                               coords.Value) == true;

        private void NotifySkillStatesChanged()
        {
            Player.OnPropertyChanged(nameof(Player.SkillingUnitSkillStates));
        }
    }
}