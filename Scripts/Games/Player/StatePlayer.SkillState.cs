using System.Collections.Generic;
using Godot;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Player;

public partial class StatePlayer
{
    private class SkillState(StatePlayer player) : State(player)
    {
        private SkillEvent? _currentSkillEvent;
        private IReadOnlyList<ISkill> _validSkills;
        private ISkill? _currentSkill;
        private int _currentSkillIndex = 0;
        private Vector2I[]? _currentSkillArgs;
        private IReadOnlyUnitState? _unitState;
        
        private HashSet<Vector2I> _validSkillCoords = new();
        private bool _validSkillCoordsDirty = true;
        public override void OnEnter()
        {
            _unitState = GameState?.UnitMapState.GetUnit(FocusedUnitId);
        }
        public override void OnInput<T>(in T input)
        {
            base.OnInput(in input);
            switch (input)
            {
                case PlayerInputClick click:
                    
                    break;
                case PlayerInputMousceCoords coords:
                    
                    break;
            }
        }

        private void TryBeginNewSkill(int index)
        {
            if (index < 0 || index >= _validSkills.Count) return;
             _currentSkill = _validSkills[index];
            _currentSkillEvent = EventFactory.CreateSkill(FocusedUnitId, _currentSkill);
            _currentSkillIndex = 0;
            _currentSkillArgs = new Vector2I[_currentSkill.ArgCount];
            _validSkillCoordsDirty = true;
        }

        private void SetCurrentIndexArg(in Vector2I arg)
        {
            if (_validSkillCoordsDirty)
            {
                _validSkillCoords.Clear();
                
            }
        }
    }
}