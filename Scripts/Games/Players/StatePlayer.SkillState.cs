using System.Linq;
using TheBlockWar.Scripts.Games.Buffs;
using TheBlockWar.Scripts.Games.Events;
using TheBlockWar.Scripts.Games.GameStates;
using TheBlockWar.Scripts.Games.Simulations;
using TheBlockWar.Scripts.Games.Skills;
using TheBlockWar.Scripts.Resources;
using TheBlockWar.Scripts.Tools;
using TheBlockWar.Scripts.Views;

namespace TheBlockWar.Scripts.Games.Players;

public partial class StatePlayer
{
    private class SkillingState(StatePlayer player) : State(player)
    {
        public int CurrentSkillIndex { get; set; } = 0;
        private SkillEvent? _currentSkillEvent;
        private IReadOnlyList<ISkill> _validSkills = [];
        private ISkill _currentSkill = null!;
        private int _currentArgIndex = -1;
        private Vector2I?[]? _currentSkillArgs;
        private IReadOnlyUnitState _unitState = UnitState.Null;
        private long _validRegionId;
        private HashSet<Vector2I> _validRegionCoords = new();
        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId) ?? UnitState.Null;
            _validSkills = GameState?.GetUnitValidSkills(_unitState.Name) ?? [];
            //从这里面通知SkillStates改变
            TryBeginNewSkill(CurrentSkillIndex);
        }

        public override void OnExit()
        {
            base.OnExit();
            Player.GameView.ClearRegion();
            _validSkills = [];
            _currentSkill = null!;
            _currentSkillEvent = null;
            _unitState = UnitState.Null;
            _currentSkillArgs = null;
            _validRegionId = 0;
            _currentArgIndex = -1;
            _validRegionCoords.Clear();
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
                case PlayerInputSkill skill:
                    if (skill.Index < 0 || skill.Index >= _validSkills.Count ||
                        !_unitState.GetShouldUseSkill(GameState, _validSkills[skill.Index])) return;
                    Logger.Info("选择技能{skill}", skill.Index);
                    EventScheduler!.DeleteEvent(_currentSkillEvent!);
                    Player.GameView.ClearRegion();
                    CurrentSkillIndex = skill.Index;
                    Player._stateMachine.ResetState();
                    break;
            }
        }

        private void TryBeginNewSkill(int index)
        {
            if (TrySelectSkill(index)) return;
            var i = 0;
            for (; i < _validSkills.Count; i++)
                if (TrySelectSkill(i)) break;
            if (i == _validSkills.Count)
                StateMachine.ChangeState(Player._freeState);

            bool TrySelectSkill(int index)
            {
                _currentSkill = _validSkills[index];
                if (_currentSkill.ArgCount == 0 || !_unitState.GetShouldUseSkill(GameState, _currentSkill)) return false;
                for (var j = 0; j < Player._skillStates.Length; j++) 
                    Player._skillStates[j].IsSelecting = false;
                Player._skillStates[index].IsSelecting = true;
                NotifySkillStatesChanged();
                TryEnterToNextArg();
                return true;
            }
        }


        private void TryEnterToNextArg()
        {
            if (_currentArgIndex == -1) //进入第一个参数
            {
                //创建技能
                _currentSkillEvent = EventScheduler!.CreateEvent(id => new SkillEvent(id)
                {
                    SkillBindUnitId = Player.SelectedUnitId,
                    SkillName = _currentSkill.SkillName,
                    Team = _unitState.Team,
                    PhaseOrder = Player.PhaseOrder,
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
                Player.GameView.ClearRegion();
                _currentSkillEvent!.Layer = SimulationLayer.Running;
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
                Player.GameView.ClearRegion();
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
            Player.GameView.ClearRegion();
            _validRegionCoords =
                _currentSkill
                    .GetSkillArgs(GameState!, _unitState, _unitState.Coords, _currentArgIndex, _currentSkillArgs!)
                    .Select(a => a + _unitState.Coords).ToHashSet();
            Player.GameView.ShowRegion(_validRegionCoords, _unitState.Team, RegionShowTypeEnum.Skill);
        }

        private bool IsCurrentValidArg()
        {
            return _currentSkillArgs![_currentArgIndex] is not null &&
                   _validRegionCoords.Contains(_currentSkillArgs![_currentArgIndex]!.Value + _unitState.Coords);
        }

        private bool IsArgGlobalValid(Vector2I? coords) => coords is not null &&
                                                           _validRegionCoords.Contains(coords.Value);

        private void NotifySkillStatesChanged()
        {
            Player.OnPropertyChanged(nameof(Player.SkillingUnitSkillStates));
        }
    }
}