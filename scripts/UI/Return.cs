	using Godot;

public partial class Return : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	public void OnPressedReturn(){
		GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
		// var global = GetNode<Global>("/root/Global");
		// global.ReturnToPreviousScene();
		var parent = (Control)GetParent();
		var higherParent = parent.GetParent();
		
		if (higherParent.GetType() == typeof(Node2D))
		{	
			var p = (Node2D)higherParent;
			p.Visible = false;
			return;
		}
		parent.Visible = false;

	}
}
