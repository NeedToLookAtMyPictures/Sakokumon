using Godot;

public partial class DebugSceneNav : Button
{
	public void OnPressedDebugNav(string path)
	{
		GetNode<Global>("/root/Global").GoToScene(path);
	}
}
