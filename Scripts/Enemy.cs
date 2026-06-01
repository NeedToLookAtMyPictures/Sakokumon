using Godot;
using System;

public partial class Enemy : Node3D
{
    // Example: Access the Player node using a relative path from Enemy
    public override void _Ready()
    {
        var player = GetNode<Node3D>("../Player");
        GD.Print($"Enemy sees player at: {player.GlobalTransform.origin}");
    }
}
