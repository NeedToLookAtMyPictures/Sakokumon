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
			case "allow":
				GD.Print("TODO: Allow NPC passage and update game state");
				break;
			case "detain":
				GD.Print("TODO: Detain subject and update game state");
				break;
			case "inspect":
				global.GoToScene("res://scenes/common/item_inspection.tscn");
				break;
			default:
				GD.PushWarning($"Invalid button ID '{id}' in NpcInteractionOptions.cs");
				break;
		}
	}
}
