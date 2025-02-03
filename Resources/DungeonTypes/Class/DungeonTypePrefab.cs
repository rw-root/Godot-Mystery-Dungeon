using Godot;

[GlobalClass]
public partial class DungeonTypePrefab : Resource
{
	[ExportGroup("Grid Tiling")]
	[Export]
	public bool FixTilesX;
	[Export]
	public int TilesX = 4;
	[Export]
	public bool FixTilesY;
	[Export]
	public int TilesY = 4;
	[ExportGroup("Coverage")]
	[Export]
	public int CoverageX = 100;
	[Export]
	public int CoverageY = 100;
	[Export]
	public int CoverageOffsetX = 0;
	[Export]
	public int CoverageOffsetY = 0;
}
