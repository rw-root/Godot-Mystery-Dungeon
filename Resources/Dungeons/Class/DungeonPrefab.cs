using Godot;

[GlobalClass]
public partial class DungeonPrefab : Resource
{
	[Export]
	public DungeonTypePrefab DungeonType;
	[ExportGroup("Generation")]
	[Export]
	public int Floors = 5;
	[Export]
	public bool SecondaryGeneration = false;
	[Export]
	public ESecondary SecondaryType;
	[ExportGroup("Weather")]
	[Export]
	public bool SpawnWeather = false;
	[Export]
	public EWeather Weather;
	[Export]
	public int WeatherChance = 0;
	[ExportGroup("Tileset")]
	[Export]
	public TileSet[] Tilesets;
	[Export]
	public bool TilesetSwap = false;
	[Export]
	public int TilesetSwapFloor = 5;
	[ExportGroup("Rooms")]
	[Export]
	public int RoomDensity = 6;
}
