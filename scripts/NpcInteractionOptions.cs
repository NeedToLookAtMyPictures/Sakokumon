using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class NpcInteractionOptions : Button
{
	
	public async void OnPressedNpcOption(string id)
	{
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		var global = GetNode<Global>("/root/Global");
		var Animate = async () =>
		{
			GetNode<Control>("/root/NpcInteraction/Control/ActionControl").Visible = false;
			
			var npcSprite = GetNode<Sprite2D>("/root/NpcInteraction/NpcSprite");
			var tween = CreateTween();
			tween.TweenProperty(npcSprite,"position:x",1494.0,2.0f);
			await ToSignal(tween,Tween.SignalName.Finished);
			npcSprite.Texture = new PlaceholderTexture2D(); 

		};
		switch (id)
		{
			case "illegal_list":
				
				global.GoToScene("res://scenes/common/illegal_items.tscn");
				break;
			case "allow":
				global.State = Data.GameState.NPCAllowed;
				await Animate();
				global.ReleaseGuardpost(depart: true);
				Global.Instance.nodesInStorage = new List<Node2D>();
				Global.Instance.itemsInStorage = new List<ObjectData>();
				Global.Instance.itemsInGrid = new List<ObjectData>();
				// global.ReturnToPreviousScene();
				
				break;
			case "detain":
				global.State = Data.GameState.NPCDenied;
				Global.Instance.nodesInStorage = new List<Node2D>();
				Global.Instance.itemsInStorage = new List<ObjectData>();
				Global.Instance.itemsInGrid = new List<ObjectData>();
				await Animate();
				global.ReleaseGuardpost(depart: false);
				// global.ReturnToPreviousScene();
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
