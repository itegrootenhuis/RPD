using Godot;
using System;
using System.Collections.Generic;

public partial class GameState : Node
{
	// Deterministic RNG: seed once at run start
	public RandomNumberGenerator Rng { get; private set; } = new();

	// Run config
	public int MaxRounds { get; private set; } = 8;

	// Players
	public int PlayerCount { get; private set; } = 2;
	public List<PlayerState> Players { get; } = new();

	// Turn/round
	public TurnSystem Turn { get; private set; } = new(2, 8);

	// Content (could be loaded from JSON later)
	public BoardDef Board { get; private set; } = DemoContent.MakeDemoBoard();

	public override void _Ready()
	{
		// Do nothing here; call StartNewRun() from Game scene.
	}

	public void StartNewRun(int playerCount, int maxRounds, ulong seed)
	{
		PlayerCount = Mathf.Clamp(playerCount, 1, 4);
		MaxRounds = Mathf.Max(1, maxRounds);

		Players.Clear();
		for (int i = 0; i < PlayerCount; i++)
		{
			Players.Add(new PlayerState { Id = i, BoardIndex = 0, Hp = 30, MaxHp = 30, Gold = 0 });
		}

		Rng = new RandomNumberGenerator();
		Rng.Seed = (long)seed;

		Turn = new TurnSystem(PlayerCount, MaxRounds);
		Board = DemoContent.MakeDemoBoard();
	}
}

public sealed class PlayerState
{
	public int Id { get; set; }
	public int BoardIndex { get; set; }
	public int Hp { get; set; }
	public int MaxHp { get; set; }
	public int Gold { get; set; }
}

public sealed class BoardDef
{
	public List<TileDef> Tiles { get; set; } = new();
	public int BossIndex { get; set; }
}

public enum TileType { Start, Battle, Heal, Gold, Item, Skill, Minigame, Casino, Trap, Camp, Shop, Elite, Boss }

public sealed class TileDef
{
	public TileType Type { get; set; }
	public int ParamA { get; set; }          // amount/tier/etc.
	public string? Payload { get; set; }     // id string for item/skill/minigame
}

public static class DemoContent
{
	public static BoardDef MakeDemoBoard()
	{
		var tiles = new List<TileDef>();
		tiles.Add(new TileDef { Type = TileType.Start });
		tiles.Add(new TileDef { Type = TileType.Gold, ParamA = 10 });
		tiles.Add(new TileDef { Type = TileType.Battle, ParamA = 1 });
		tiles.Add(new TileDef { Type = TileType.Heal, ParamA = 5 });
		tiles.Add(new TileDef { Type = TileType.Casino });
		tiles.Add(new TileDef { Type = TileType.Battle, ParamA = 2 });
		tiles.Add(new TileDef { Type = TileType.Camp });
		tiles.Add(new TileDef { Type = TileType.Elite, ParamA = 3 });
		tiles.Add(new TileDef { Type = TileType.Boss });
		return new BoardDef { Tiles = tiles, BossIndex = tiles.Count - 1 };
	}
}

public sealed class TurnSystem
{
	public int Round { get; private set; } = 1;
	public int MaxRounds { get; }
	private readonly List<int> _order = new();
	private int _idx = 0;

	public TurnSystem(int players, int maxRounds)
	{
		MaxRounds = maxRounds;
		for (int i = 0; i < players; i++)
		{
			_order.Add(i);
		}
	}

	public int CurrentPlayer()
	{
		return _order[_idx];
	}

	public void EndTurn()
	{
		_idx++;
		if (_idx >= _order.Count)
		{
			_idx = 0;
			Round++;
		}
	}

	public bool IsBossTime()
	{
		if (Round > MaxRounds)
		{
			return true;
		}
		else
		{
			return false;
		}
	}
}
