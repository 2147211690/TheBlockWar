using System.Collections;
using TheBlockWar.Scripts.Games.GameStates;

namespace TheBlockWar.Scripts.Games.Simulations;

public class SimResultList : IList<StepResult>, IReadOnlyList<StepResult>
{
    [Obsolete("用于序列化,不要直接使用")]
    public List<StepResult> Results { get; set; } = [];
    
    public void Replay(IGameState state)
    {
        foreach (var step in Results)
        {
            step.ReplayCore(state);
        }
    }
    public T AddResult<T>(T step) where T : StepResult
    {
        Results.Add(step);
        return step;
    }
    
    public IEnumerator<StepResult> GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Results.GetEnumerator();
    }

    public void Add(StepResult item)
    {
        Results.Add(item);
    }

    public void Clear()
    {
        Results.Clear();
    }

    public bool Contains(StepResult item)
    {
        return Results.Contains(item);
    }

    public void CopyTo(StepResult[] array, int arrayIndex)
    {
        Results.CopyTo(array, arrayIndex);
    }

    public bool Remove(StepResult item)
    {
        return Results.Remove(item);
    }

    public int Count => Results.Count;
    public bool IsReadOnly => false;

    public int IndexOf(StepResult item)
    {
        return Results.IndexOf(item);
    }

    public void Insert(int index, StepResult item)
    {
        Results.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        Results.RemoveAt(index);
    }

    public StepResult this[int index]
    {
        get => Results[index];
        set => Results[index] = value;
    }
}