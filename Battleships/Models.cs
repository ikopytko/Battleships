namespace Battleships;

public enum CellStatus : byte
{
    Unknown,
    Hit,
    Miss,
    Ship
}

public enum MoveResult
{
    Invalid,
    Hit,
    Miss
}

public readonly record struct Cell(int X, int Y)
{
    public static Cell From(int x, int y) => new(x, y);
}