using Godot;

public partial class NpcInteractionOptions : Button
{
	public void OnPressedNpcOption(string id)
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		var global = GetNode<Global>("/root/Global");

		switch (id)
		{
			case "illegal_list":
				
				global.GoToScene("res://scenes/common/illegal_items.tscn");
				break;
			case "inspect":
				global.State = Data.GameState.ItemsInspected;
				global.GoToScene("res://scenes/common/item_inspection.tscn");
				break;
			default:
				GD.PushWarning($"Invalid button ID '{id}' in NpcInteractionOptions.cs");
				break;
		}
	}
}
