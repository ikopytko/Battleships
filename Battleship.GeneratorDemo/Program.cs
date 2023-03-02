using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Battleships;

using Bitmap bitmap = new Bitmap(/*iteration*/ 21 * /*pixel size*/ 2 * ( /*board size*/ 10 + /*offset*/ 1),
    21 * 2 * (10 + 1), PixelFormat.Format32bppArgb);
using Graphics graphics = Graphics.FromImage(bitmap);
using var blackPen = new Pen(Color.Black, 1);
using var whitePen = new Pen(Color.White, 1);
graphics.FillRectangle(new SolidBrush(Color.Wheat), new Rectangle(0, 0, bitmap.Width, bitmap.Height));


Stopwatch sw = Stopwatch.StartNew();
for (var y = 0; y < 20; y++)
{
    for (var x = 0; x < 20; x++)
    {
        (Board board, _)  = new BoardBuilder()
            .AddShip(5)
            .AddShip(4)
            .AddShip(4)
            .AddShip(2)
            .Build();

        for (var by = 0; by < 10; by++)
        {
            for (var bx = 0; bx < 10; bx++)
            {
                if (board[bx, by] != 0)
                {
                    graphics.DrawRectangle(
                        blackPen, 2 + x * 23 + bx * 2, 2 + y * 23 + by * 2, 1, 1);
                }
                else
                {
                    graphics.DrawRectangle(
                        whitePen, 2 + x * 23 + bx * 2, 2 + y * 23 + by * 2, 1, 1);
                }
            }
        }
    }
}
sw.Stop();
Console.WriteLine(sw.ElapsedMilliseconds);
bitmap.Save($"Test Image.png", ImageFormat.Png);
