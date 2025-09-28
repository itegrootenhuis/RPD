using Godot;
using System;
using System.Collections.Generic;

public partial class BoardGenerator : Node2D
{
	[Export] public int TilesPerSide = 10;   // 10 → 40 tiles
	[Export] public float TileWidth = 200f;
	[Export] public float TileHeight = 100f;
	[Export] public float Padding = 10f;

	[Export] public PackedScene TileScene;
	[Export] public PackedScene CornerTileScene;

	private readonly Random _rng = new Random();

	public override void _Ready()
	{
		if (TileScene == null || CornerTileScene == null)
		{
			GD.PushError("TileScene or CornerTileScene not assigned.");
			return;
		}

		int c0 = GetCornerIndex(0); // bottom-left
		int c1 = GetCornerIndex(1); // bottom-right
		int c2 = GetCornerIndex(2); // top-right
		int c3 = GetCornerIndex(3); // top-left

		int index = 0;

		// Bottom row (left → right)
		for (int i = 0; i < TilesPerSide; i++)
		{
			var pos = new Vector2(i * (TileWidth + Padding), (TilesPerSide - 1) * (TileHeight + Padding));
			AddTile(index++, pos, 0, c0, c1, c2, c3);
		}

		// Right column (bottom → top, skip bottom-right)
		for (int i = TilesPerSide - 2; i >= 0; i--)
		{
			var pos = new Vector2((TilesPerSide - 1) * (TileWidth + Padding), i * (TileHeight + Padding));
			AddTile(index++, pos, 1, c0, c1, c2, c3);
		}

		// Top row (right → left, skip top-right)
		for (int i = TilesPerSide - 2; i >= 0; i--)
		{
			var pos = new Vector2(i * (TileWidth + Padding), 0);
			AddTile(index++, pos, 2, c0, c1, c2, c3);
		}

		// Left column (top → bottom, skip top-left & bottom-left)
		for (int i = 1; i < TilesPerSide - 1; i++)
		{
			var pos = new Vector2(0, i * (TileHeight + Padding));
			AddTile(index++, pos, 3, c0, c1, c2, c3);
		}
	}

	private void AddTile(int index, Vector2 position, int side, int c0, int c1, int c2, int c3)
	{
		Node2D node;

		if (index == c0 || index == c1 || index == c2 || index == c3)
		{
			node = CornerTileScene.Instantiate<Node2D>();
			node.Name = $"CornerTile{index}";
		}
		else
		{
			node = TileScene.Instantiate<Node2D>();
			node.Name = $"Tile{index}";
			// TODO: add randomization if needed
		}

		node.Position = position;

		// Rotate based on which side of the board we’re on
		// Bottom row → face up (0°)
		// Right column → face left (90°)
		// Top row → face down (180°)
		// Left column → face right (270°)
		if (side == 0)
		{
			node.RotationDegrees = 0;
		}
		else if (side == 1)
		{
			node.RotationDegrees = -90;
		}
		else if (side == 2)
		{
			node.RotationDegrees = 180;
		}
		else if (side == 3)
		{
			node.RotationDegrees = 90;
		}

		AddChild(node);
	}

	private int GetCornerIndex(int corner)
	{
		int step = TilesPerSide - 1;
		if (corner == 0) return 0;          // bottom-left
		else if (corner == 1) return step;  // bottom-right
		else if (corner == 2) return step * 2; // top-right
		else return step * 3;               // top-left
	}
}
