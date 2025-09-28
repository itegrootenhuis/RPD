using Godot;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Board : Node2D
{
	[Signal] public delegate void PawnArrivedEventHandler(int playerId, int tileIndex);

	[Export] public NodePath TileContainerPath;
	[Export] public NodePath PlayerPawnsPath;

	private Node2D _tileContainer;
	private Node2D _pawnsRoot;
	private List<Node2D> _tiles = new();
	private List<Node2D> _pawns = new();

	public override void _Ready()
	{
		_tileContainer = GetNode<Node2D>(TileContainerPath);
		_pawnsRoot = GetNode<Node2D>(PlayerPawnsPath);

		_tiles.Clear();
		foreach (var child in _tileContainer.GetChildren())
		{
			if (child is Node2D n2)
			{
				_tiles.Add(n2);
			}
		}

		_pawns.Clear();
		foreach (var child in _pawnsRoot.GetChildren())
		{
			if (child is Node2D n2)
			{
				_pawns.Add(n2);
			}
		}
	}

	public void SetPawnToTile(int playerId, int tileIndex)
	{
		var pawn = _pawns[playerId];
		var tile = _tiles[tileIndex % _tiles.Count];
		pawn.GlobalPosition = tile.GlobalPosition;
	}

	public async Task MovePawnAsync(int playerId, int steps, float perStepSeconds = 0.25f)
	{
		var state = GameStateSingleton();
		var pawn = _pawns[playerId];
		var tiles = _tiles;
		var currentIndex = state.Players[playerId].BoardIndex;
		var total = tiles.Count;

		for (int i = 1; i <= steps; i++)
		{
			int next = (currentIndex + i) % total;
			var to = tiles[next].GlobalPosition;
			await TweenToAsync(pawn, to, perStepSeconds);
		}

		var finalIndex = (currentIndex + steps) % total;
		state.Players[playerId].BoardIndex = finalIndex;

		EmitSignal(SignalName.PawnArrived, playerId, finalIndex);
	}

	private static async Task TweenToAsync(Node2D node, Vector2 target, float duration)
	{
		var tween = node.CreateTween();
		tween.TweenProperty(node, "global_position", target, duration);
		await node.ToSignal(tween, Tween.SignalName.Finished);
	}

	private static GameState GameStateSingleton()
	{
		var gs = (GameState)Engine.GetMainLoop().Root.GetNode("/root/GameState");
		return gs;
	}
}
