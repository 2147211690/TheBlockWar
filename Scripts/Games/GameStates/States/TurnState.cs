using TheBlockWar.Scripts.Games.GameStates.Datas;

namespace TheBlockWar.Scripts.Games.GameStates;

public class TurnState : ITurnState
{
    public int CurrentTurn { get; set; }
    public Dictionary<int, Dictionary<string, int>> PhaseOrders { get; } = new();
    public int GetPhaseOrder(int turn, string phaseName)
    {
        return PhaseOrders[turn][phaseName];
    }

    public TurnState(int currentTurn = -1)
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
            CurrentTurn = CurrentTurn,
            PhaseOrder = PhaseOrders
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
        public Dictionary<int, Dictionary<string, int>> PhaseOrders => Owner.PhaseOrders;

        public int GetPhaseOrder(int turn, string phaseName)
        {
            return Owner.GetPhaseOrder(turn, phaseName);
        }

        public TurnStateSnapshot(ITurnState owner)
        {
            Owner = owner;
            CurrentTurn = Owner.CurrentTurn;
        }
        public TurnStateData ToData()
        {
            return new TurnStateData
            {
                CurrentTurn = CurrentTurn,
                PhaseOrder = PhaseOrders
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
