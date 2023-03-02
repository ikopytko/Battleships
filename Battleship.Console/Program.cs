using Battleships;

var boardB = new BoardBuilder()
    .AddShip(5)
    .AddShip(4)
    .AddShip(4)
    .AddShip(2);

var game = new Game(boardB, new CheatConsolePrinter());

for (int i = 0; i < 10; i++)
    game.Shot(Cell.From(i, i));

Console.WriteLine(game.ShipsLeft());

game.Print();

public class ConsolePrinter : IPrinter
{
    public void Print(Board board)
    {
        for (int y = 0; y < board.BoardSize; y++)
        {
            for (int x = 0; x < board.BoardSize; x++)
            {
                Console.Write(GetPrintableChar(board[x, y]));
            }

            Console.WriteLine();
        }
    }

    protected virtual char GetPrintableChar(CellStatus cell) => cell switch
    {
        CellStatus.Hit => 'X',
        CellStatus.Miss => 'O',
        _ => '.'
    };
}

public class CheatConsolePrinter : ConsolePrinter
{
    protected override char GetPrintableChar(CellStatus cell) => cell switch
    {
        CellStatus.Ship => '#',
        _ => base.GetPrintableChar(cell)
    };
}