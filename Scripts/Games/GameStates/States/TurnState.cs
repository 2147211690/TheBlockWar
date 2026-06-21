using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TurnState : ITurnState
{
    public int CurrentTurn { get; set; }

    public TurnState(int currentTurn = 0)
    {
        CurrentTurn = currentTurn;
    }
        
    public TurnState(TurnStateData data)
    {
        CurrentTurn = data.CurrentTurn;
    }

    public TurnStateData ToData()
    {
        return new TurnStateData
        {
            CurrentTurn = CurrentTurn
        };
    }

    public ITurnState GetSnapshot()
    {
        return new TurnStateSnapshot(this);
    }

    public void ClearIfSnapshot()
    {
    }
    
    class TurnStateSnapshot : ITurnState
    {
        private ITurnState Owner { get; }
        public int CurrentTurn { get; set; }
        public TurnStateSnapshot(ITurnState owner)
        {
            Owner = owner;
            CurrentTurn = Owner.CurrentTurn;
        }
        public TurnStateData ToData()
        {
            return new TurnStateData
            {
                CurrentTurn = CurrentTurn
            };
        }

        public ITurnState GetSnapshot()
        {
            return new TurnStateSnapshot(this);
        }


        public void ClearIfSnapshot()
        {
            CurrentTurn = Owner.CurrentTurn;
        }
    }
}
