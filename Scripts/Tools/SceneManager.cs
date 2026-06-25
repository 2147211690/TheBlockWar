namespace TheBlockWar.Scripts.Tools;

public partial class SceneManager : Node
{
    private static SceneManager _instance;

    protected SceneManager()
    {
        if (_instance != null)
        {
            QueueFree();
        }
        else _instance = this;
    }
    
    public static void ChangeSceneToPacked(PackedScene scene, Action<Node>? afterChanged = null)
    {
        _instance.GetTree().SceneChanged += OnSceneChanged;

        void OnSceneChanged()
        {
            _instance.GetTree().SceneChanged -= OnSceneChanged;
            afterChanged?.Invoke(_instance.GetTree().CurrentScene);
        }

        _instance.GetTree().ChangeSceneToPacked(scene);
    }
}