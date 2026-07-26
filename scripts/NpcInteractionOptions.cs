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
				{
					var npc = global.ActiveNpcs.Find(n => n.CurrentState == Global.NpcData.State.AtGuardpost);
					if (npc != null)
						npc.CurrentState = Global.NpcData.State.Departing;
					global.npcPresent = false;
				}
				break;
			case "detain":
				{
					var npc = global.ActiveNpcs.Find(n => n.CurrentState == Global.NpcData.State.AtGuardpost);
					if (npc != null)
						global.ActiveNpcs.Remove(npc);
					global.npcPresent = false;
				}
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
