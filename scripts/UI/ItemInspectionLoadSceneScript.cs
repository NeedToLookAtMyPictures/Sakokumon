using Godot;
using System;

public partial class ItemInspectionLoadSceneScript : Node2D
{
	private Vector2 originalViewportSize;
	// set new viewport size for item inspection scene
	public override void _EnterTree()
	{
		Viewport viewport = GetViewport();
		if (viewport != null)
		{
			Rect2 tempRect = viewport.GetVisibleRect();
			originalViewportSize = tempRect.Size;

			tempRect.Size = new Vector2(648,846);
		}
	}

	// reset old viewport size for other scenes
	public override void _ExitTree()
	{
		Viewport viewport = GetViewport();
		if (viewport != null)
		{
			Rect2 tempRect = viewport.GetVisibleRect();
			tempRect.Size = originalViewportSize;
		}
	}
}
