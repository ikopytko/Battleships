namespace Battleships;

public interface IPrinter
{
    void Print(Board board);
}

public class Game
{
    private readonly IPrinter _printer;
    private readonly Board _board;
    private readonly List<List<Cell>> _ships;

    public Game(BoardBuilder boardBuilder, IPrinter printer)
    {
        _printer = printer;
        (_board, _ships) = boardBuilder.Build();
    }

    public MoveResult Shot(Cell target)
    {
        // bounds
        switch (_board[target])
        {
            case CellStatus.Hit:
            case CellStatus.Miss:
                return MoveResult.Invalid;
            case CellStatus.Unknown:
                _board[target] = CellStatus.Miss;
                return MoveResult.Miss;
            case CellStatus.Ship:
                _board[target] = CellStatus.Hit;
                return MoveResult.Hit;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Print() => _printer.Print(_board);

    public int ShipsLeft() => _ships.Count(x => x.Any(c => _board[c] == CellStatus.Ship));
}

public class BoardBuilder
{
    private readonly int _boardSize;

    enum Orientation
    {
        Horizontal,
        Vertical
    }

    private readonly List<int> _shipsToAdd = new();

    public BoardBuilder(int boardSize = 10)
    {
        _boardSize = boardSize;
    }

    public BoardBuilder AddShip(int shipSize)
    {
        _shipsToAdd.Add(shipSize);

        return this;
    }

    public (Board, List<List<Cell>>) Build()
    {
        var board = Board.Empty(_boardSize);
        var fleet = new List<List<Cell>>();
        foreach (var shipSize in _shipsToAdd.Order())
        {
            var segment = GetFreeSegment(board, shipSize);

            if (segment.HasValue)
            {
                (Cell segmentStart, int segmentLength, Orientation orientation) = segment.Value;

                var (startCell, endCell) = GetShipPosition(shipSize, segmentLength, orientation, segmentStart);

                fleet.Add(PlaceShip(board, startCell, endCell));
            }
        }

        return (board, fleet);
    }

    public List<Cell> PlaceShip(Board board, in Cell startCell, in Cell endCell)
    {
        //int shipId = Ships.Count + 1;
        //Ships.Add(shipId, shipSize);
        List<Cell> ship = new List<Cell>();
        for (int x = startCell.X; x <= endCell.X; x++)
        {
            for (int y = startCell.Y; y <= endCell.Y; y++)
            {
                var cell = Cell.From(x, y);
                ship.Add(cell);
                board[cell] = CellStatus.Ship;
            }
        }

        return ship;
    }

    /// <summary>
    /// For the given board segment try to find a subset of the cells for the ship of given size.
    /// <param name="segmentLength"/> must be greater or equals than <param name="shipSize"/>.
    /// </summary>
    private static (Cell startCell, Cell endCell) GetShipPosition(int shipSize, int segmentLength,
        Orientation orientation,
        in Cell segmentStart)
    {
        Cell startCell, endCell;
        var offset = Random.Shared.Next(segmentLength - shipSize);
        switch (orientation)
        {
            case Orientation.Horizontal:
                startCell = segmentStart with {X = segmentStart.X + offset};
                endCell = startCell with {X = startCell.X + shipSize - 1};
                break;
            case Orientation.Vertical:
                startCell = segmentStart with {Y = segmentStart.Y + offset};
                endCell = startCell with {Y = startCell.Y + shipSize - 1};
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return (startCell, endCell);
    }

    /// <summary>
    /// Try to find a random segment of the board that can fit a ship of the given <paramref name="shipSize"/>
    /// </summary>
    private (Cell SegmentStart, int SegmentLength, Orientation Orientation)? GetFreeSegment(Board board, int shipSize)
    {
        const int retries = 10;
        foreach (var (currentPivot, orientation) in GetLineToCheck(retries, bruteforceDiagonals: true))
        {
            var contiguousSpace = 0;
            Dictionary<Cell, int> freeSegments = new();
            Cell? lastCell = null;
            foreach (var cell in GetAxisEnumerator(orientation, currentPivot))
            {
                if (IsClear(board, cell))
                {
                    if (contiguousSpace == 0)
                    {
                        lastCell = cell;
                        freeSegments[lastCell.Value] = 0;
                    }

                    freeSegments[lastCell!.Value]++;
                    contiguousSpace++;
                }
                else
                {
                    contiguousSpace = 0;
                }
            }

            var placingVariants = freeSegments.Count(x => x.Value >= shipSize);
            if (placingVariants == 0)
            {
                Console.WriteLine("No luck. Trying next combination");
                continue;
            }

            var segment = freeSegments.Where(x => x.Value >= shipSize).Skip(Random.Shared.Next(placingVariants))
                .FirstOrDefault();

            return (segment.Key, segment.Value, orientation);
        }

        Console.WriteLine("Out of possible solution in reasonable iteration count");
        return null;
    }

    /// <summary>
    /// Get a random horizontal or vertical line of the board.
    /// </summary>
    private IEnumerable<(Cell pivot, Orientation orientation)> GetLineToCheck(int retries, bool bruteforceDiagonals)
    {
        if (bruteforceDiagonals) retries += _boardSize;

        // Enumerable.Range(0, 9).OrderBy(_ => Random.Shared.Next()).Select(x => Cell.From(x, x));
        for (int i = 0; i < retries; i++)
        {
            var pivot = GetRandomCell();
            if ((pivot.X + pivot.Y) % 2 == 0)
            {
                yield return (pivot, Orientation.Vertical);
                yield return (pivot, Orientation.Horizontal);
            }
            else
            {
                yield return (pivot, Orientation.Horizontal);
                yield return (pivot, Orientation.Vertical);
            }
            //var orientation = Enum.GetValues<Orientation>()[Random.Shared.Next(2)];
        }
    }

    /// <summary>
    /// Verify that given cell and all 8 surrounding cell are free from other ships
    /// </summary>
    private bool IsClear(Board board, in Cell cell)
    {
        for (int x = cell.X - 1; x <= cell.X + 1; x++)
        {
            for (int y = cell.Y - 1; y <= cell.Y + 1; y++)
            {
                if (x < 0 || x >= _boardSize || y < 0 || y >= _boardSize)
                    continue;
                if (board[x, y] == CellStatus.Ship)
                    return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns an iterator for an entire row or a column that passes through a given cell 
    /// </summary>
    IEnumerable<Cell> GetAxisEnumerator(Orientation axis, Cell cell)
    {
        for (var i = 0; i < _boardSize; i++)
        {
            yield return axis switch
            {
                Orientation.Horizontal => cell with {X = i},
                Orientation.Vertical => cell with {Y = i},
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    Cell GetRandomCell()
    {
        return Cell.From(
            Random.Shared.Next(_boardSize),
            Random.Shared.Next(_boardSize));
    }
}

public class Board
{
    public readonly int BoardSize;

    private CellStatus[,] Grid { get; }
    //public Dictionary<int, int> Ships { get; } = new();

    public Board(int boardSize)
    {
        BoardSize = boardSize;
        Grid = new CellStatus[BoardSize, BoardSize];
    }

    public CellStatus this[Cell cell]
    {
        get => Grid[cell.X, cell.Y];
        set => Grid[cell.X, cell.Y] = value;
    }

    public CellStatus this[int x, int y]
    {
        get => Grid[x, y];
        set => Grid[x, y] = value;
    }

    public static Board Empty(int boardSize) => new(boardSize);
}