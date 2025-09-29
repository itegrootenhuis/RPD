using Godot;
using System;
using System.Collections.Generic;

public partial class BoardGenerator : Node2D
{
	// ---- Assign in the Inspector ----
	[ExportCategory("Scenes")]
	[Export] public PackedScene StartCornerScene { get; set; } = null!;            // Corner 1 (fixed "Start")
	[Export] public Godot.Collections.Array<PackedScene> OtherCornerScenes { get; set; } = new(); // Pool for corners 2,3,4
	[Export] public Godot.Collections.Array<PackedScene> EdgeTileScenes { get; set; } = new();    // Pool for all 36 non-corner tiles

	[ExportCategory("Layout")]
	[Export] public int CornerSize { get; set; } = 160;   // Must fit four 32×32 pawns (≥ 64); 160 gives comfy room
	[Export] public int EdgeWidth { get; set; } = 160;    // Along the side direction (top/bottom run left→right)
	[Export] public int EdgeHeight { get; set; } = 80;    // Depth from the edge inward
	[Export] public int Pad { get; set; } = 10;           // Gap between tiles
	[Export] public bool CenterInViewport { get; set; } = true;

	// ---- Internals ----
	private const int Between = 9; // 9 rectangles between corners per side (9*4 = 36)
	private Control _uiLayer = null!;
	private Node2D _n2Layer = null!;
	private readonly RandomNumberGenerator _rng = new();

	private enum Side { Top, Right, Bottom, Left }

	public override void _Ready()
	{
		_rng.Randomize();
		Generate();
	}

	private void Generate()
	{
		// Basic validation
		if (StartCornerScene == null)
		{
			GD.PushError("StartCornerScene (Corner 1) is not assigned.");
			return;
		}
		if (OtherCornerScenes == null || OtherCornerScenes.Count == 0)
		{
			GD.PushError("OtherCornerScenes must contain at least 1 scene to randomize corners 2–4.");
			return;
		}
		if (EdgeTileScenes == null || EdgeTileScenes.Count == 0)
		{
			GD.PushError("EdgeTileScenes must contain at least 1 scene to randomize edge tiles.");
			return;
		}

		EnsureLayers(); // clean previous runs and recreate layers

		// Board span (square): 2 corners + 9 edge tiles + 10 pads (one between each neighbor)
		float span = (CornerSize * 2) + (Between * EdgeWidth) + ((Between + 1) * Pad);

		Vector2 origin = Vector2.Zero;
		if (CenterInViewport)
		{
			Vector2 vp = GetViewportRect().Size;
			origin = (vp - new Vector2(span, span)) * 0.5f;
			if (origin.X < 0) { origin.X = 0; }
			if (origin.Y < 0) { origin.Y = 0; }
		}

		float left = origin.X;
		float top = origin.Y;
		float right = origin.X + span - CornerSize;   // top-left for right-side corners
		float bottom = origin.Y + span - CornerSize;  // top-left for bottom-side corners

		int idx = 0;

		// ---- Corners (clockwise indexing: 0..39) ----
		// Corner 1: Top-Left (Start) — fixed
		SpawnCorner(StartCornerScene, new Vector2(left, top), idx++, label: "Start");

		// Corner 2: Top-Right — randomized from pool
		var c2 = PickRandom(OtherCornerScenes);
		SpawnCorner(c2, new Vector2(right, top), idx++, label: "Corner2");

		// Corner 3: Bottom-Right — randomized from pool
		var c3 = PickRandom(OtherCornerScenes);
		SpawnCorner(c3, new Vector2(right, bottom), idx++, label: "Corner3");

		// Corner 4: Bottom-Left — randomized from pool
		var c4 = PickRandom(OtherCornerScenes);
		SpawnCorner(c4, new Vector2(left, bottom), idx++, label: "Corner4");

		// ---- Edges ----
		// Top row (between Corner1 and Corner2), left -> right
		for (int i = 0; i < Between; i++)
		{
			float x = left + CornerSize + Pad + i * (EdgeWidth + Pad);
			SpawnEdge(new Vector2(x, top), Side.Top, idx++);
		}

		// Right column (between Corner2 and Corner3), top -> bottom
		for (int i = 0; i < Between; i++)
		{
			float y = top + CornerSize + Pad + i * (EdgeWidth + Pad);
			SpawnEdge(new Vector2(right, y), Side.Right, idx++);
		}

		// Bottom row (between Corner3 and Corner4), right -> left
		for (int i = 0; i < Between; i++)
		{
			float x = right - Pad - EdgeWidth - i * (EdgeWidth + Pad);
			SpawnEdge(new Vector2(x, bottom), Side.Bottom, idx++);
		}

		// Left column (between Corner4 and Corner1), bottom -> top
		for (int i = 0; i < Between; i++)
		{
			float y = bottom - Pad - EdgeWidth - i * (EdgeWidth + Pad);
			SpawnEdge(new Vector2(left, y), Side.Left, idx++);
		}

		// Safety: ensure we produced exactly 40 (4 + 36)
		GD.Print($"Board generated {idx} tiles (expect 40).");
	}

	// ---------- Helpers ----------

	private void EnsureLayers()
	{
		// Remove old layers if re-running
		foreach (Node child in GetChildren())
		{
			if (child.Name == "TileUILayer" || child.Name == "TileN2Layer")
			{
				child.QueueFree();
			}
		}

		_uiLayer = new Control
		{
			Name = "TileUILayer",
			LayoutMode = 0, // Anchors
			AnchorLeft = 0, AnchorTop = 0, AnchorRight = 1, AnchorBottom = 1,
			OffsetLeft = 0, OffsetTop = 0, OffsetRight = 0, OffsetBottom = 0
		};
		// IMPORTANT: plain Control, not a Container
		AddChild(_uiLayer);

		_n2Layer = new Node2D { Name = "TileN2Layer" };
		AddChild(_n2Layer);
	}

	private PackedScene PickRandom(Godot.Collections.Array<PackedScene> pool)
	{
		int i = (int)_rng.RandiRange(0, pool.Count - 1);
		return pool[i];
	}

	private void SpawnCorner(PackedScene scene, Vector2 topLeft, int index, string label)
	{
		Node inst = scene.Instantiate<Node>();
		inst.Name = $"Tile{index:00}_Corner_{label}";

		if (inst is Control c)
		{
			_uiLayer.AddChild(c);
			SetupControl(c, new Vector2(CornerSize, CornerSize), topLeft);
		}
		else if (inst is Node2D n2)
		{
			_n2Layer.AddChild(n2);
			SetupNode2D(n2, new Vector2(CornerSize, CornerSize), topLeft, 0f);
		}
		else
		{
			_n2Layer.AddChild(inst);
		}
	}

	private void SpawnEdge(Vector2 topLeft, Side side, int index)
	{
		// Randomize which edge tile to use for this position
		PackedScene scene = PickRandom(EdgeTileScenes);
		Node inst = scene.Instantiate<Node>();
		inst.Name = $"Tile{index:00}_{side}";

		bool vertical = side == Side.Left || side == Side.Right;

		if (inst is Control c)
		{
			_uiLayer.AddChild(c);
			// For Control tiles, avoid rotation: swap W/H on vertical sides to keep text upright
			Vector2 size = vertical ? new Vector2(EdgeHeight, EdgeWidth) : new Vector2(EdgeWidth, EdgeHeight);
			SetupControl(c, size, topLeft);
		}
		else if (inst is Node2D n2)
		{
			_n2Layer.AddChild(n2);
			float rot = side == Side.Top ? 0f :
						side == Side.Right ? Mathf.Pi * 0.5f :
						side == Side.Bottom ? Mathf.Pi :
						Mathf.Pi * 1.5f;
			SetupNode2D(n2, new Vector2(EdgeWidth, EdgeHeight), topLeft, rot);
		}
		else
		{
			_n2Layer.AddChild(inst);
		}
	}

	private static void SetupControl(Control c, Vector2 size, Vector2 topLeft)
	{
		// Kill layout/resize behavior so Containers cannot stretch tiles
		c.LayoutMode = 0; // Anchors
		c.AnchorLeft = 0; c.AnchorTop = 0; c.AnchorRight = 0; c.AnchorBottom = 0;
		c.OffsetLeft = 0; c.OffsetTop = 0; c.OffsetRight = 0; c.OffsetBottom = 0;
		c.SizeFlagsHorizontal = 0; c.SizeFlagsVertical = 0;
		c.CustomMinimumSize = Vector2.Zero;
		c.Scale = Vector2.One;

		// Root size/position; child ColorRect set to "Full Rect" will match this
		c.Size = size;
		c.Position = topLeft;
		c.Visible = true;
		c.ZIndex = 1;
	}

	private static void SetupNode2D(Node2D n2, Vector2 size, Vector2 topLeft, float rotation)
	{
		n2.Scale = Vector2.One;
		n2.Position = topLeft + (size * 0.5f); // rotate around center
		n2.Rotation = rotation;
		n2.ZIndex = 1;
	}
}
