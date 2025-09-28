using Godot;

public enum CornerType
{
	Go,
	BossGate,
	Jail,
	FreeParking,
	HealStation,
	Shop,
	MinigameHub
}

public partial class CornerTile : Node2D
{
	[Export] public CornerType Type = CornerType.Go;
	[Export] public string Payload = "";

	private Control _visual;

	public override void _Ready()
	{
		_visual = GetNodeOrNull<Control>("ColorRect") ?? GetNodeOrNull<Control>("Panel");
		ApplyVisual();
	}

	private void ApplyVisual()
	{
		if (_visual is ColorRect rect)
		{
			if (Type == CornerType.Go)
			{
				rect.Color = Colors.DarkGreen;
			}
			else if (Type == CornerType.BossGate)
			{
				rect.Color = Colors.Purple;
			}
			else if (Type == CornerType.Jail)
			{
				rect.Color = Colors.Blue;
			}
			else if (Type == CornerType.FreeParking)
			{
				rect.Color = Colors.Orange;
			}
			else if (Type == CornerType.HealStation)
			{
				rect.Color = Colors.LightGreen;
			}
			else if (Type == CornerType.Shop)
			{
				rect.Color = Colors.Yellow;
			}
			else if (Type == CornerType.MinigameHub)
			{
				rect.Color = Colors.Cyan;
			}
			else
			{
				rect.Color = Colors.Gray;
			}
		}
	}
}
