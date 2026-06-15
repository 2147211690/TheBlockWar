namespace TheBlockWar.Scripts.Tools;

public class RecordTimer
{
    public ulong StartTick { get; private set; }
    public float Time { get; set; }
    public RecordTimer(float time)
    {
        Time = time;
        Restart();
    }
    public bool HasTime => GetNormalizedTime() <= 1;
    public float GetNormalizedTime() => (Godot.Time.GetTicksMsec() - StartTick) / (Time * 1000);
    public void Restart()
    {
        StartTick = Godot.Time.GetTicksMsec();
    }
}