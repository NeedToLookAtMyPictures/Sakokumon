using Godot;

public partial class NewGameOptions : Button
{
	public async void OnPressedBegin()
	{
		var tween = CreateTween();
		var parent = GetParent();
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		tween.TweenProperty(parent,"modulate",new Color(0,0,0,1),3.0f)
			.SetTrans(Tween.TransitionType.Sine)
			.SetEase(Tween.EaseType.Out);
		await ToSignal(tween,Tween.SignalName.Finished);
		GetNode<Global>("/root/Global").GoToScene("res://scenes/common/harbor_view.tscn");
	}
}
