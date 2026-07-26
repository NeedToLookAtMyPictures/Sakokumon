using Godot;

public partial class ContinueDay : Button
{
    public override void _Ready()
    {
        Visible = GetNode<Global>("/root/Global").DayComplete;
    }

    public void OnPressedContinue()
    {
        GetNode<MusicManager>("/root/MusicManager").PlayButtonSfx();
        var global = GetNode<Global>("/root/Global");
        global.NextDay();
        global.GoToScene("res://scenes/common/harbor_view.tscn");
    }
}
