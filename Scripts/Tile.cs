using Godot;

public partial class Tile : Node2D
{
	[Export] public TileType Type = TileType.Battle;
	[Export] public int ParamA = 0;
	[Export] public string Payload = "";

	// optional: quick reference to child ColorRect or Panel
	private Control _visual;

	public override void _Ready()
	{
		_visual = GetNode<Control>("ColorRect");
		ApplyColor();
	}

	private void ApplyColor()
	{
		if (_visual is ColorRect rect)
		{
			switch (Type)
			{
				case TileType.Battle: rect.Color = Colors.Red; break;
				case TileType.Heal:   rect.Color = Colors.Green; break;
				case TileType.Gold:   rect.Color = Colors.Yellow; break;
				default:              rect.Color = Colors.Gray; break;
			}
		}
	}
}
