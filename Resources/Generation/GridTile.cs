using Godot;
using System;

[GlobalClass]
public partial class GridTile : Resource
{
	[Export]
	public EGridTileType Type;
	[Export]
	public bool Filled;
	[Export]
	public Vector2I Start;
	[Export]
	public Vector2I End;
	
	public void SetGridTile(EGridTileType InputType, bool InputFilled, Vector2I InputStart, Vector2I InputEnd)
	{
		Type = InputType;
		Filled = InputFilled;
		Start = InputStart;
		End = InputEnd;
	}
}
