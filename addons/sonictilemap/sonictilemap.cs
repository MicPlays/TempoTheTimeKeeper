#if TOOLS
using Godot;
using System;

[Tool]
public partial class sonictilemap : EditorPlugin
{

	private WorldGrid currentWorldGrid;
	private EditorInterface editorInterface;
	private Control worldGridPane;
	public override void _EnterTree()
	{
		this.SceneChanged += OnSceneChanged;
		this.editorInterface = EditorInterface.Singleton;

		//find WorldGrid of scene that is loaded on startup
		Node loadedSceneRoot = editorInterface.GetEditedSceneRoot();
		setCurrentWorldGrid(loadedSceneRoot);

		//Add UI Pane
		worldGridPane = GD.Load<PackedScene>("res://addons/sonictilemap/WorldGridUI.tscn").Instantiate<Control>();
		AddControlToBottomPanel(worldGridPane, "WorldGrid");
		
		//Add WorldGrid Node as custom node
		var script = GD.Load<Script>("res://addons/sonictilemap/WorldGrid.cs");
		var texture = GD.Load<Texture2D>("res://addons/sonictilemap/TileMap.svg");
		AddCustomType("WorldGrid", "Node2D", script, texture);

	}


	public override void _ExitTree()
	{
		// Clean-up of the plugin goes here.
		RemoveCustomType("WorldGrid");
		RemoveControlFromBottomPanel(worldGridPane);
		worldGridPane.Free();
	}

	public void OnSceneChanged(Node sceneRoot)
	{
		setCurrentWorldGrid(sceneRoot);
	}
    public override bool _Handles(GodotObject @object)
    {
        return true;
    }

    public override bool _ForwardCanvasGuiInput(InputEvent @event)
    {
		if (@event is InputEventMouseMotion eventMouseMotion)
            {
				if (editorInterface.GetSelection().GetSelectedNodes().Contains(currentWorldGrid))
				{
					currentWorldGrid.xGridSizePixels = 128f * currentWorldGrid.xChunkSize;
					currentWorldGrid.yGridSizePixels = 128f * currentWorldGrid.yChunkSize;

					Vector2 localMousePos = currentWorldGrid.GetLocalMousePosition();
					//if mouse is in gridspace
					if ((localMousePos.X <= currentWorldGrid.xGridSizePixels && localMousePos.X >= 0) && 
					(localMousePos.Y <= currentWorldGrid.yGridSizePixels && localMousePos.Y >= 0)) 
					{
						int xSquare = (int)(localMousePos.X / 128f);
						int ySquare = (int)(localMousePos.Y / 128f);
						GD.Print(new Vector2(xSquare, ySquare));
					}
				}
            }
		return false;
	}

	public void setCurrentWorldGrid(Node sceneRoot)
	{
		var rootChildren = sceneRoot.FindChildren("WorldGrid");
		if (rootChildren.Count == 0)
		{
			//prompt user to create WorldGrid node
		}
		else if (rootChildren.Count > 1)
		{
			//prompt user to only have one WorldGrid node
		}
		else 
		{
			this.currentWorldGrid = (WorldGrid)rootChildren[0];
		}
	}

}
#endif
