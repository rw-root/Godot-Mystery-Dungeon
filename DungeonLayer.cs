using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public partial class DungeonLayer : TileMapLayer
{
	[ExportGroup("Dungeon Prefab")]
	// Sets the Dungeon we are generating Floors for
	[Export]
	private DungeonPrefab Dungeon;
	// Current Floor we are Generating
	[Export]
	private int Floor = 1;
	[ExportGroup("Seed")]
	[Export]
	public bool UseSeed;
	[Export]
	public ulong Seed;
	[ExportGroup("Grid")]
	// Sets Dimensions of the TileMap Grid to Generate
	[Export]
	public int MaxX = 56;
	[Export]
	public int MaxY = 32;
	[ExportGroup("Rooms")]
	// is supposed to Determine how many Rooms are on a Floor, 
	//ToDo: Move into the Dungeon Prefab and maybe 
	//		make it a curve for when its supposed to get more over Floors
	[Export]
	public int RoomDensity = 6;
	// Just a General Rule of how many Rooms are Generated
	[Export]
	public int MinRooms = 2;
	[Export]
	public int MaxRooms = 36;
	// Redundand ToDo: remove completely
	[Export]
	public int RoomMaxX = 14;
	// Redundand ToDo: remove completely
	[Export]
	public int RoomMaxY = 6;
	// minimum size of rooms are 4x4 and they are currently turned into Anchors
	// ToDo: 	Split Room and Anchor Placers, still place them both in the GenerateRooms() step
	// 			But combine it with Generating Rooms up to the designated Amount
	[Export]
	public int AnchorThreshhold = 20;
	
	// The Size of the Cells in the Room Grid
	// ToDo: Rename so its more obvious... but the whole project could use that
	public int GridSizeX;
	public int GridSizeY;
	// A List of All Cells Valid for Room/Anchor Generation
	public List<GridTile> GridTiles;
	
	// For all the RNG this is gonna take...
	public RandomNumberGenerator RNG = new RandomNumberGenerator();
	
	// Some Tiles Hardcoded because its a Placeholder SpriteSheet
	// ToDo: Maybe make it Dynamic? i seriously have no clue...
	private Vector2I AtlasDirt = new Vector2I(1,1);
	private Vector2I AtlasGrass = new Vector2I(4,0);
	private Vector2I AtlasStone = new Vector2I(3,4);
	
	// Nodes i need for Displaying UI
	private LineEdit SeedInput;
	private Label FloorLabel;
	
	// Variables i wanted outside the functions for some reason
	private int GeneratedRooms;
	private int TilesetAtlas;
	
	// Inisialisation of things
	public override void _Ready()
	{
		GetNode<CheckButton>("%UseSeedToggle").ButtonPressed = UseSeed;
		SeedInput = GetNode<LineEdit>("%SeedInput");
		FloorLabel = GetNode<Label>("%FloorLabel");
		FloorLabel.Text = "" + Floor;
		
		//Start First Generation Automagically
		GenerateDungeon();
	}
	
	// Generation Steps
	public void GenerateDungeon()
	{
		// ToDo: Create Full Generation Loop
		// Randomize
		if(UseSeed == false)
		{
			RNG.Randomize();
		}
		else
		{
			RNG.SetSeed(Seed);
		}
		GD.Print("Generation Seed: " + RNG.Seed);
		Seed = RNG.Seed;
		SeedInput.Text = Seed.ToString();
		
		GridTiles = new List<GridTile>();
		
		// Set Tileset for current Floor
		if(Floor >= Dungeon.TilesetSwapFloor && Dungeon.TilesetSwap)
		{
			this.TileSet = Dungeon.Tilesets[1];
			TilesetAtlas = 1;
			GD.Print("ALTERNATE TILESET");
		}
		else
		{
			this.TileSet = Dungeon.Tilesets[0];
			TilesetAtlas = 0;
			GD.Print("STANDARD TILESET");
		}
		
		// Set Size for Grid
		if(Dungeon.DungeonType.FixTilesX)
		{
			GridSizeX = (int) Math.Floor((double) MaxX / Dungeon.DungeonType.TilesX);
		}
		else
		{
			GridSizeX = (int) Math.Floor((double) MaxX / RNG.RandiRange(2,7));
		}
		
		if(Dungeon.DungeonType.FixTilesY)
		{
			GridSizeY = (int) Math.Floor((double) MaxY / Dungeon.DungeonType.TilesY);
		}
		else
		{
			GridSizeY = (int) Math.Floor((double) MaxY / RNG.RandiRange(2,4));
		}
		
		// Start Generation with resetting the Grid
		FillGrid(AtlasGrass);
		
		// Get all the valid GridTiles
		GetValidGridTiles();
		
		//Start Generating the Rooms
		if(GenerateRooms(MinRooms, MaxRooms) == false)
		{
			GD.Print("Rooms could not be Generated, Try Again!");
			GD.Print("Initial Dungeon Generation Aborted!");
			return;
		}
		
		GenerateHallways();
		
		GetNode<Label>("%InfoText").Text = "---> Seed: " + Seed + " --- Rooms: " + GeneratedRooms + " <---";
		GD.Print("Initial Dungeon Generation Complete!");
		return;
	}
	
	// Fill the Tilemap with Tiles and Mark Cell Borders with Other Tiles, only for debugging ofc
	private void FillGrid(Vector2I Tile)
	{
		for( int CurrentX = 0; CurrentX < MaxX ; CurrentX++)
		{
			for( int CurrentY = 0; CurrentY < MaxY; CurrentY++)
			{
				if(
				CurrentX % GridSizeX == 0 || CurrentY % GridSizeY == 0 ||
				CurrentX % GridSizeX == GridSizeX - 1 || CurrentY % GridSizeY == GridSizeY - 1)
				{
					this.SetCell(new Vector2I(CurrentX,CurrentY), TilesetAtlas, AtlasStone, 0);
				}
				else
				{
					this.SetCell(new Vector2I(CurrentX,CurrentY), TilesetAtlas, Tile, 0);
				}
			}
		}
		GD.Print("Grid Filled!");
		return;
	}
	
	// Fill List with the Grid Cells that are Completely Valid and can be used for Generation
	private void GetValidGridTiles()
	{
		int StartX = 0 + Dungeon.DungeonType.CoverageOffsetX;
		int StartY = 0 + Dungeon.DungeonType.CoverageOffsetY;
		
		for(int GridCurrentX = 0; GridCurrentX < MaxX; GridCurrentX += GridSizeX)
		{
			for(int GridCurrentY = 0; GridCurrentY < MaxY; GridCurrentY += GridSizeY)
			{
				if(CheckArea(new Vector2I(GridCurrentX + 1, GridCurrentY + 1), new Vector2I(GridCurrentX + GridSizeX - 1, GridCurrentY + GridSizeY - 1), TilesetAtlas))
				{
					GridTile Tile = new GridTile();
					Tile.SetGridTile(EGridTileType.EMPTY, false, new Vector2I(GridCurrentX + 1, GridCurrentY + 1), new Vector2I(GridCurrentX + GridSizeX - 1, GridCurrentY + GridSizeY - 1));
					GridTiles.Add(Tile);
				}
			}
		}
		int ForEachI = 0;
		foreach(GridTile Tile in GridTiles)
		{
			GD.Print("Tile " + ForEachI + " is from: X: " + Tile.Start.X + " Y: " + Tile.Start.Y + " --- To: X: " + Tile.End.X + " Y: " + Tile.End.Y);
			ForEachI++;
		}
	}
	
	// Loop Through GridTiles and place a Room or Anchor
	// ToDo: Only Generate "RoomDesity + [0..2]" Rooms, Generate Anchors.
	private bool GenerateRooms(int MinAmount, int MaxAmount)
	{
		int RoomAmount = Dungeon.RoomDensity + RNG.RandiRange(0, 2);
		GD.Print("Amount of Rooms: " + RoomAmount);
		int RoomsPlaced = 0;
		
		foreach(GridTile Tile in GridTiles)
		{
			int RoomPlacementX = RNG.RandiRange(4, Tile.End.X - Tile.Start.X);
			int RoomPlacementY = RNG.RandiRange(4, Tile.End.Y - Tile.Start.Y);
			if(RoomPlacementX * RoomPlacementY < AnchorThreshhold)
			{
				RoomPlacementX = 1;
				RoomPlacementY = 1;
				Tile.Type = EGridTileType.ANCHOR;
			}
			
			int RoomStartX = RNG.RandiRange(Tile.Start.X, Tile.End.X - RoomPlacementX);
			int RoomStartY = RNG.RandiRange(Tile.Start.Y, Tile.End.Y - RoomPlacementY);
			
			if(PlaceRoom(new Vector2I(RoomStartX, RoomStartY), RoomPlacementX, RoomPlacementY))
			{
				if(RoomPlacementX != 1 && RoomPlacementY != 1)
				{
					RoomsPlaced++;
					GD.Print("Placed Room");
				}
				else
				{
					GD.Print("Placed Anchor");
				}
			}
		}
		
		GeneratedRooms = RoomsPlaced;
		GD.Print(RoomsPlaced + "/" + RoomAmount + " Rooms have been Placed");
		return true;
	}
	
	// Connect Rooms and Anchors with Hallways
	// ToDo: See Above
	public void GenerateHallways()
	{
		GD.Print("" + (GridSizeX * GridSizeY) + " --- " + GridTiles[0].Start.X);
	}
	
	// Place Hallway between Two points either in L shape or Z shape
	// ToDo: See Above
	private void PlaceHallway(Vector2I Start, Vector2I End)
	{
		
	}
	
	// Place a room of Dimensions (RoomX, RoomY) at the Coordinates RoomCoordinate
	private bool PlaceRoom(Vector2I RoomCoordinate ,int RoomX ,int RoomY)
	{
		if(CheckArea(RoomCoordinate, new Vector2I(RoomCoordinate.X + RoomX,RoomCoordinate.Y + RoomY), TilesetAtlas))
		{
			for(int RoomPlacementX = 0; RoomPlacementX < RoomX; RoomPlacementX++)
			{
				for(int RoomPlacementY = 0; RoomPlacementY < RoomY; RoomPlacementY++)
				{
					this.SetCell(new Vector2I(RoomCoordinate.X + RoomPlacementX,RoomCoordinate.Y + RoomPlacementY), TilesetAtlas, AtlasDirt, 0);
				}
			}
			GD.Print("Room Placed!");
			return true;
		}
		GD.Print("Room Placement Aborted...");
		return false;
	}
	
	// Check if Area from Start(X,Y) to End(X,Y) is inside the Grid
	private bool CheckArea(Vector2I Start, Vector2I End, int CheckFor)
	{
		int StartX = Start.X;
		int StartY = Start.Y;
		int EndX = End.X;
		int EndY = End.Y;
		GD.Print("Checking Area:\n --> Start: X:" + StartX + " Y:" + StartY + " --- End: X:" + EndX + " Y: " + EndY);
		if(StartX > EndX && StartY > EndY)
		{
			GD.Print("When checking an Area, the End Coords were in front of the Start Coords..."
			 + "\n --> Start: X:" + StartX + " Y:" + StartY + " --- End: X:" + EndX + " Y: " + EndY);
			return false;
		}
		for(int CheckX = StartX; CheckX < EndX; CheckX++)
		{
			for(int CheckY = StartY; CheckY < EndY; CheckY++)
			{
				Vector2I CheckCoords = new Vector2I(CheckX, CheckY);
				int TileSourceId = this.GetCellSourceId(CheckCoords);
				Vector2I TileAtlas = this.GetCellAtlasCoords(CheckCoords);
				if(TileSourceId != CheckFor)
				{
					GD.Print("A Checked Tile was found Invalid...\n"
					+ " ---> X: " + CheckCoords.X + " Y: " + CheckCoords.Y + "\n"
					+ " ---> TileSourceId = " + TileSourceId + "\n"
					+ " ---> TileAtlasCoords = X: " + TileAtlas.X + " Y: " + TileAtlas.Y);
					return false;
				}
			}
		}
		return true;
	}
	
	// Getter and Setter
	public bool GetUseSeed()
	{
		return UseSeed;
	}
	public void SetUseSeed(bool Value)
	{
		UseSeed = Value;
	}
	public ulong GetSeed()
	{
		return Seed;
	}
	public void SetSeed(string Value)
	{
		Seed = (ulong) UInt64.Parse(Value);
	}
	public int GetFloor()
	{
		return Floor;
	}
	public void SetFloor(int Value)
	{
		Floor = Value;
		FloorLabel.Text = "" + Floor;
	}
	public void FloorUp()
	{
		if(Floor >= 999)
		{
			return;
		}
		Floor++;
		FloorLabel.Text = "" + Floor;
	}
	public void FloorDown()
	{
		if(Floor <= 1)
		{
			return;
		}
		Floor--;
		FloorLabel.Text = "" + Floor;
	}
	
}
