using Godot;
using System;

public partial class Player : Node3D
{
    // Example: Move the player relative to its sibling "Enemy"
    public override void _Ready()
    {
        // Find the sibling Enemy node using a relative path
        var enemy = GetNode<Node3D>("../Enemy");
        GD.Print($"Found enemy at position: {enemy.GlobalTransform.origin}");
    }

    public override void _Process(double delta)
    {
        // Simple forward movement
        Translate(Vector3.Forward * (float)(5 * delta));
    }
}
