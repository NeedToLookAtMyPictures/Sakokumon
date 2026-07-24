using Godot;

public partial class NewGameOptions : Button
{
	public void OnPressedBegin()
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		GetNode<Global>("/root/Global").GoToScene("res://scenes/common/npc_interaction.tscn");
	}
}
