using static System.Console;

var width = 0;
while (width == 0)
{
    Write("Width: ");
    _ = int.TryParse(ReadLine(), out width);
}

var height = 0;
while (height == 0)
{
    Write("Height: ");
    _ = int.TryParse(ReadLine(), out height);
}

var bombs = -1;
while (bombs == -1)
{
    Write("Bombs: ");
    _ = int.TryParse(ReadLine(), out bombs);
}

var grid = new List<Square>();
var random = new Random();

for (int i = 0; i < bombs; i++)
{
    int x, y = 0;
    do
    {
        x = random.Next(0, width);
        y = random.Next(0, height);
    }
    while (grid.Any(g => g.X == x && g.Y == y));

    grid.Add(new()
    {
        IsBomb = true,
        X = x,
        Y = y
    });
}

for (int x = 0; x < width; x++)
{
    for (int y = 0; y < height; y++)
    {
        if (grid.Any(g => g.X == x && g.Y == y))
        {
            continue;
        }

        grid.Add(new()
        {
            IsBomb = false,
            X = x,
            Y = y,
            AdjacentBombs = grid.Count(g => g.X >= x - 1 && g.X <= x + 1 && g.Y >= y - 1 && g.Y <= y + 1 && g.IsBomb)
        });
    }
}

var result = Result.Ongoing;

bool Expose(Square? square)
{
    if (square is null)
    {
        return false;
    }

    if (square.IsExposed)
    {
        return true;
    }

    square.IsExposed = true;

    if (!square.IsBomb && square.AdjacentBombs == 0)
    {
        foreach (var s in grid.Where(g => g.X >= square.X - 1 && g.X <= square.X + 1 && g.Y >= square.Y - 1 && g.Y <= square.Y + 1 && !g.IsExposed))
        {
            Expose(s);
        }
    }

    return true;
}

void Print()
{
    Clear();

    for (int y = 0; y < height; y++)
    {
        Write("|");
        for (int x = 0; x < width; x++)
        {
            Write("---|");
        }
        WriteLine();

        Write("|");
        for (int x = 0; x < width; x++)
        {
            Write(grid.Single(g => g.X == x && g.Y == y) switch
            {
                { IsExploded: true } => " @ |",
                { IsExposed: true, AdjacentBombs: 0 } => "   |",
                { IsExposed: true } g => $" {g.AdjacentBombs} |",
                { IsFlagged: true } => " # |",
                var g => $"{g.Name}|"
            });
        }
        WriteLine();
    }

    Write("|");
    for (int x = 0; x < width; x++)
    {
        Write("---|");
    }
    WriteLine();
}

do
{
    Print();

    var isMoveValid = false;
    do
    {
        WriteLine();
        Write(">: ");
        isMoveValid = ReadLine()!.Split(' ').Select(s => s.Trim().ToLower()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() switch
        {
            [var squareName] => Expose(grid.SingleOrDefault(g => g.Name.ToLower() == squareName)),
            ["f", var squareName] => (grid.SingleOrDefault(g => g.Name.ToLower() == squareName)?.IsFlagged = true) ?? false,
            _ => false
        };
    }
    while (!isMoveValid);

    result = (grid.Any(g => g.IsExploded), grid.Any(g => !g.IsResolved)) switch
    {
        (true, _) => Result.Lost,
        (_, true) => Result.Ongoing,
        _ => Result.Won
    };
}
while (result == Result.Ongoing);

Print();

WriteLine(result switch
{
    Result.Won => "You won!",
    _ => "You lost :("
});

WriteLine("Press any key to continue...");
ReadKey();

enum Result
{
    Ongoing,
    Won,
    Lost
}

class Square
{
    public required bool IsBomb { get; init; }

    public required int X { get; init; }

    public required int Y { get; init; }

    public int AdjacentBombs { get; init; } = 0;

    public string Name =>
        $"{(char)('A' + Y)}{(X + 1).ToString("D2")}";

    public bool IsFlagged { get; set; }

    public bool IsExposed { get; set; }

    public bool IsResolved =>
        (IsBomb && IsFlagged)
        || (!IsBomb && IsExposed);

    public bool IsExploded =>
        IsBomb && IsExposed;
}
