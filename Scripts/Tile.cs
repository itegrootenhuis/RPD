using Godot;

public partial class Tile : Node2D
{
	[Export] public TileType Type = TileType.Battle;
	[Export] public int ParamA = 0;
	[Export] public string Payload = "";

	private Control _visual;

	public override void _Ready()
	{
		_visual = GetNodeOrNull<Control>("ColorRect") ?? GetNodeOrNull<Control>("Panel");
		ApplyColor();
	}

	// Call this if you change Type after the node is in the tree
	public void RefreshVisual()
	{
		if (_visual == null)
		{
			_visual = GetNodeOrNull<Control>("ColorRect") ?? GetNodeOrNull<Control>("Panel");
		}
		ApplyColor();
	}

	private void ApplyColor()
	{
		if (_visual is ColorRect rect)
		{
			if (Type == TileType.Battle)
			{
				rect.Color = Colors.DarkRed;
			}
			else if (Type == TileType.Heal)
			{
				rect.Color = Colors.LawnGreen;
			}
			else if (Type == TileType.Gold)
			{
				rect.Color = Colors.Yellow;
			}
			else
			{
				rect.Color = Colors.Gray;
			}
		}
	}
}
