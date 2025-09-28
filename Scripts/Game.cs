using Godot;
using System.Threading.Tasks;

public partial class Game : Node
{
	[Export] public NodePath BoardPath;
	[Export] public NodePath HudPath;

	private Board _board;
	private HUD _hud;
	private GameState _gs;

	public override void _Ready()
	{
		_gs = GetGameState();
		_board = GetNode<Board>(BoardPath);
		_hud = GetNode<HUD>(HudPath);

		// WIRE SIGNALS
		_board.PawnArrived += OnPawnArrived;
		_hud.RollClicked += OnRollClicked;

		// Start a fresh run
		// Seed can be random or settable for tests
		ulong seed = (ulong)GD.Randi();
		_gs.StartNewRun(playerCount: 2, maxRounds: 8, seed: seed);

		// Place pawns on Start tile
		for (int i = 0; i < _gs.PlayerCount; i++)
		{
			_board.SetPawnToTile(i, _gs.Players[i].BoardIndex);
		}

		UpdateHud();
		_hud.AppendLog("New run started.");
		_hud.SetRollEnabled(true);
	}

	private void UpdateHud()
	{
		_hud.SetTurnInfo(_gs.Turn.CurrentPlayer(), _gs.Turn.Round, _gs.MaxRounds);
	}

	private async void OnRollClicked()
	{
		// Roll dice for current player
		int pid = _gs.Turn.CurrentPlayer();
		int roll = RollD6();

		_hud.AppendLog($"Player {pid + 1} rolled {roll}.");
		_hud.SetRollEnabled(false);

		await _board.MovePawnAsync(pid, roll, 0.2f);
		// When movement finishes, OnPawnArrived is called (signal).
	}

	private async void OnPawnArrived(int playerId, int tileIndex)
	{
		var tile = _gs.Board.Tiles[tileIndex];

		// Resolve tile
		await ResolveTileAsync(playerId, tile);

		// After resolution, advance the turn (unless boss pulled us into an end state)
		if (_gs.Turn.IsBossTime())
		{
			_hud.AppendLog("Boss time! Warping to boss encounter...");
			await StartBossEncounterAsync();
			return;
		}

		_gs.Turn.EndTurn();
		UpdateHud();
		_hud.SetRollEnabled(true);
	}

	private async Task ResolveTileAsync(int playerId, TileDef tile)
	{
		switch (tile.Type)
		{
			case TileType.Gold:
				_gs.Players[playerId].Gold += tile.ParamA;
				_hud.AppendLog($"+{tile.ParamA} gold to Player {playerId + 1} (Total: {_gs.Players[playerId].Gold}).");
				break;

			case TileType.Heal:
				var p = _gs.Players[playerId];
				int before = p.Hp;
				p.Hp = Mathf.Clamp(p.Hp + tile.ParamA, 0, p.MaxHp);
				_hud.AppendLog($"Heal {tile.ParamA} for Player {playerId + 1} ({before} → {p.Hp}).");
				break;

			case TileType.Battle:
			case TileType.Elite:
				await StartBattleAsync(playerId, tier: Mathf.Max(1, tile.ParamA));
				break;

			case TileType.Camp:
				_hud.AppendLog("Camp: choose a small buff (placeholder).");
				break;

			case TileType.Casino:
				await StartCasinoAsync(playerId);
				break;

			case TileType.Boss:
				_hud.AppendLog("You stepped on the Boss tile!");
				await StartBossEncounterAsync();
				break;

			default:
				_hud.AppendLog($"Tile {tile.Type} (not implemented yet).");
				break;
		}
	}

	// ————— Encounters (stubs you’ll replace with real scenes) —————
	private async Task StartBattleAsync(int playerId, int tier)
	{
		_hud.AppendLog($"Battle (tier {tier}) for Player {playerId + 1}…");

		// Placeholder auto-battle sim; replace with proper scene.
		bool win = _gs.Rng.Randf() > 0.35f;
		await ToSignal(GetTree().CreateTimer(0.8f), SceneTreeTimer.SignalName.Timeout);

		if (win)
		{
			_hud.AppendLog("Victory! +10 gold.");
			_gs.Players[playerId].Gold += 10;
		}
		else
		{
			_hud.AppendLog("Defeat! -5 HP.");
			_gs.Players[playerId].Hp -= 5;
			if (_gs.Players[playerId].Hp <= 0)
			{
				_hud.AppendLog($"Player {playerId + 1} is down. (MVP: add revive rules.)");
			}
		}
	}

	private async Task StartCasinoAsync(int playerId)
	{
		_hud.AppendLog("Casino: rolling…");
		await ToSignal(GetTree().CreateTimer(0.6f), SceneTreeTimer.SignalName.Timeout);

		float r = _gs.Rng.Randf();
		if (r < 0.5f)
		{
			_hud.AppendLog("Won +15 gold!");
			_gs.Players[playerId].Gold += 15;
		}
		else
		{
			_hud.AppendLog("Lost 10 gold!");
			_gs.Players[playerId].Gold = Mathf.Max(0, _gs.Players[playerId].Gold - 10);
		}
	}

	private async Task StartBossEncounterAsync()
	{
		// MVP: just a coin flip with a bit of drama.
		_hud.AppendLog("Boss encounter begins!");
		await ToSignal(GetTree().CreateTimer(1.2f), SceneTreeTimer.SignalName.Timeout);

		float r = _gs.Rng.Randf();
		if (r > 0.5f)
		{
			_hud.AppendLog("You defeated the Boss! GG!");
			await EndRunAsync(true);
		}
		else
		{
			_hud.AppendLog("The Boss crushed you. Try again!");
			await EndRunAsync(false);
		}
	}

	private async Task EndRunAsync(bool win)
	{
		_hud.SetRollEnabled(false);
		await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);
		GetTree().ReloadCurrentScene();
	}

	private int RollD6()
	{
		// Use singleton RNG to stay deterministic
		int roll = (int)(_gs.Rng.Randi() % 6) + 1;
		if (roll < 1)
		{
			roll = 1;
		}
		return roll;
	}

	private static GameState GetGameState()
	{
		var gs = (GameState)Engine.GetMainLoop().Root.GetNode("/root/GameState");
		return gs;
	}
}
