using Godot;
using System;

/// Attach this to the TileContainer node that holds all TileN children.
[Tool] // so it can run in the editor
public partial class TileContainerHelper : Node2D
{
	[Export]
	public bool AutoRename { get; set; } = true;

	[Export]
	public bool WarnOnMismatch { get; set; } = true;

	public override void _Ready()
	{
		if (!Engine.IsEditorHint())
		{
			return;
		}

		if (AutoRename)
		{
			RenameChildren();
		}

		if (WarnOnMismatch)
		{
			CheckSequential();
		}
	}

	private void RenameChildren()
	{
		int i = 0;
		foreach (var child in GetChildren())
		{
			if (child is Node2D node)
			{
				node.Name = $"Tile{i}";
				i++;
			}
		}
	}

	private void CheckSequential()
	{
		int expected = 0;
		foreach (var child in GetChildren())
		{
			if (child is Node2D node)
			{
				string expectedName = $"Tile{expected}";
				if (node.Name != expectedName)
				{
					GD.PushWarning($"TileContainer: Expected '{expectedName}', found '{node.Name}'.");
				}
				expected++;
			}
		}
	}
}
