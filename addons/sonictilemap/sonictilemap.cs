#if TOOLS
using Godot;
using System;

[Tool]
public partial class sonictilemap : EditorPlugin
{
	private Control worldGridPane;
	public override void _EnterTree()
	{
		//Add UI Pane
		worldGridPane = GD.Load<PackedScene>("res://addons/sonictilemap/WorldGridUI.tscn").Instantiate<Control>();
		AddControlToBottomPanel(worldGridPane, "WorldGrid");
		
		// Add WorldGrid Node
		var script = GD.Load<Script>("res://addons/sonictilemap/WorldGrid.cs");
		var texture = GD.Load<Texture2D>("res://addons/sonictilemap/TileMap.svg");
		AddCustomType("WorldGrid", "Node2D", script, texture);
	}

	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("WorldGrid");
		worldGridPane.Free();
	}
}
#endif
