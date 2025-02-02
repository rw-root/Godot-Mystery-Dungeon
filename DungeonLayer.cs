using Godot;
using System;

public partial class DungeonLayer : TileMapLayer
{
	[ExportGroup("Seed")]
	[Export]
	public bool UseSeed;
	[Export]
	public ulong Seed;
	[ExportGroup("Grid")]
	[Export]
	public int MaxX = 80;
	[Export]
	public int MaxY = 40;
	[ExportGroup("Rooms")]
	[Export]
	public int RoomDensity = 8;
	[Export]
	public int MinRooms = 2;
	[Export]
	public int MaxRooms = 36;
	[Export]
	public int RoomMaxX = 14;
	[Export]
	public int RoomMaxY = 6;
	
	public RandomNumberGenerator RNG = new RandomNumberGenerator();
	private Vector2I AtlasDirt = new Vector2I(1,1);
	private Vector2I AtlasGrass = new Vector2I(4,0);
	private Vector2I AtlasStone = new Vector2I(3,4);
	
	private LineEdit SeedInput;
	
	private int GeneratedRooms;
	
	public override void _Ready()
	{
		GetNode<CheckButton>("%UseSeedToggle").ButtonPressed = UseSeed;
		SeedInput = GetNode<LineEdit>("%SeedInput");
		GenerateDungeon();
	}
	
	// Generation Steps
	public void GenerateDungeon()
	{
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
		
		// Start Generation with resetting the Grid
		FillGrid(AtlasGrass);
		
		//Start Generating the Rooms
		if(GenerateRooms(MinRooms, MaxRooms) == false)
		{
			GD.Print("Rooms could not be Generated, Try Again!");
			GD.Print("Initial Dungeon Generation Aborted!");
			return;
		}
		GetNode<Label>("%InfoText").Text = "---> Seed: " + Seed + " --- Rooms: " + GeneratedRooms + " <---";
		GD.Print("Initial Dungeon Generation Complete!");
		return;
	}
	
	private void FillGrid(Vector2I Tile)
	{
		for( int CurrentX = 0; CurrentX < MaxX ; CurrentX++)
		{
			for( int CurrentY = 0; CurrentY < MaxY; CurrentY++)
			{
				if(CurrentX == 0 || CurrentY == 0 || 
				CurrentX == MaxX -1 || CurrentY == MaxY -1 ||
				CurrentX % (RoomDensity * 2) == 0 || CurrentY % (RoomDensity) == 0)
				{
					this.SetCell(new Vector2I(CurrentX,CurrentY), 0, AtlasStone, 0);
				}
				else
				{
					this.SetCell(new Vector2I(CurrentX,CurrentY), 0, Tile, 0);
				}
			}
		}
		GD.Print("Grid Filled!");
		return;
	}
	
	private bool GenerateRooms(int MinAmount, int MaxAmount)
	{
		int RoomAmount = RNG.RandiRange(MinAmount, MaxAmount);
		GD.Print("Amount of Rooms: " + RoomAmount);
		int Tries = 0;
		int RoomsPlaced = 0;
		
		for(RoomsPlaced = 0; RoomsPlaced < RoomAmount; RoomsPlaced++)
		{
			GD.Print("RoomsPlaced = " + RoomsPlaced);
			
			int RoomX = RNG.RandiRange(4, RoomMaxX);
			int RoomY = RNG.RandiRange(4, RoomMaxY);
			
			int RoomTryX = RNG.RandiRange(1, MaxX - (RoomX + 1));
			int RoomTryY = RNG.RandiRange(1, MaxY - (RoomY + 1));
			
			if(Tries >= RoomAmount * 2)
			{
				GD.Print("Ran out of RoomPlacement Tries...");
				return false;
			}
			
			if(PlaceRoom(new Vector2I(RoomTryX, RoomTryY), RoomX, RoomY) == false)
			{
				Tries++;
				RoomsPlaced--;
				GD.Print("Trying to place a Room Failed...");
			}
		}
		GeneratedRooms = RoomsPlaced;
		GD.Print(RoomsPlaced + "/" + RoomAmount + " Rooms have been Placed");
		return true;
	}
	
	private bool PlaceRoom(Vector2I RoomCoordinate ,int RoomX ,int RoomY)
	{
		if(CheckArea(RoomCoordinate, new Vector2I(RoomCoordinate.X + RoomX,RoomCoordinate.Y + RoomY)))
		{
			for(int RoomPlacementX = 0; RoomPlacementX < RoomX; RoomPlacementX++)
			{
				for(int RoomPlacementY = 0; RoomPlacementY < RoomY; RoomPlacementY++)
				{
					this.SetCell(new Vector2I(RoomCoordinate.X + RoomPlacementX,RoomCoordinate.Y + RoomPlacementY), 0, AtlasDirt, 0);
				}
			}
			GD.Print("Room Placed!");
			return true;
		}
		GD.Print("Room Placement Aborted...");
		return false;
	}
	
	private bool CheckArea(Vector2I Start, Vector2I End, int CheckFor = 0)
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
}
