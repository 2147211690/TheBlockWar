#if TOOLS
namespace TheBlockWar.addons.extendable_inspector_for_cs;

[Tool]
public partial class ExtendableInspectorPlugin : EditorPlugin {
	private core.ExtendableInspector instance;

	public override void _EnterTree() {
		instance = new core.ExtendableInspector();
		AddInspectorPlugin(instance);
	}

	public override void _ExitTree() {
		RemoveInspectorPlugin(instance);
		instance.Free();
	}
}
#endif
