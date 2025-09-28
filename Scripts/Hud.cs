using Godot;

public partial class Hud : CanvasLayer
{
	[Signal] public delegate void RollClickedEventHandler();

	[Export] public NodePath CurrentPlayerLabelPath;
	[Export] public NodePath RoundLabelPath;
	[Export] public NodePath RollButtonPath;
	[Export] public NodePath LogTextPath;

	private Label _currentPlayer;
	private Label _round;
	private Button _roll;
	private RichTextLabel _log;

	public override void _Ready()
	{
		_currentPlayer = GetNode<Label>(CurrentPlayerLabelPath);
		_round = GetNode<Label>(RoundLabelPath);
		_roll = GetNode<Button>(RollButtonPath);
		_log = GetNode<RichTextLabel>(LogTextPath);

		_roll.Pressed += () => EmitSignal(SignalName.RollClicked);
	}

	public void SetTurnInfo(int playerId, int round, int maxRounds)
	{
		_currentPlayer.Text = $"Player {playerId + 1}";
		_round.Text = $"Round {round}/{maxRounds}";
	}

	public void AppendLog(string line)
	{
		_log.AppendText(line + "\n");
	}

	public void SetRollEnabled(bool enabled)
	{
		_roll.Disabled = !enabled;
	}
}
