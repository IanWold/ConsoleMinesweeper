using static System.Console;

void SetInput(string name, int max, out int value) {
    do {
        Write($"{name}: ");
        _ = int.TryParse(ReadLine(), out value);
    }
    while (value < 1 || value > max);
}

SetInput("Width", 99, out var width);
SetInput("Height", 26, out var height);
SetInput("Bombs", width * height, out var bombs);

BackgroundColor = ConsoleColor.Black;
ForegroundColor = ConsoleColor.Gray;

var grid = new List<Square>();
var random = new Random();

for (int i = 0; i < bombs; i++) {
    int x, y = 0;
    
    do {
        (x, y) = (random.Next(0, width), random.Next(0, height));
    }
    while (grid.Any(g => g.X == x && g.Y == y));

    grid.Add(new(x, y, true));
}

for (int x = 0; x < width; x++) {
    for (int y = 0; y < height; y++) {
        if (grid.Any(g => g.X == x && g.Y == y)) {
            continue;
        }

        grid.Add(new(x, y, false, grid.Count(g => g.X >= x - 1 && g.X <= x + 1 && g.Y >= y - 1 && g.Y <= y + 1 && g.IsBomb)));
    }
}

bool Expose(Square square) {
    square.IsExposed = true;

    if (!square.IsBomb && square.AdjacentBombs == 0) {
        foreach (var s in grid.Where(g => g.X >= square.X - 1 && g.X <= square.X + 1 && g.Y >= square.Y - 1 && g.Y <= square.Y + 1 && !g.IsExposed)) {
            Expose(s);
        }
    }

    return true;
}

void Print() {
    Clear();

    for (int y = 0; y < height; y++) {
        WriteLine(
            y == 0
            ? $"┌{string.Concat(Enumerable.Repeat("───┬", width - 1))}───┐"
            : $"├{string.Concat(Enumerable.Repeat("───┼", width - 1))}───┤"
        );

        Write("│");
        for (int x = 0; x < width; x++) {
            var square = grid.Single(g => g.X == x && g.Y == y);
            var (squareText, squareColor) = square switch {
                { IsExploded: true } => (" @ ", ConsoleColor.White),
                { IsExposed: true, AdjacentBombs: 0 } => ("   ", ForegroundColor),
                { IsExposed: true } => (
                    $" {square.AdjacentBombs} ",
                    square.AdjacentBombs switch {
                        1 => ConsoleColor.Blue,
                        2 => ConsoleColor.Green,
                        3 => ConsoleColor.Red,
                        4 => ConsoleColor.DarkBlue,
                        5 => ConsoleColor.Magenta,
                        6 => ConsoleColor.Cyan,
                        7 => ConsoleColor.DarkGreen,
                        _ => ConsoleColor.DarkYellow
                    }
                ),
                { IsFlagged: true } => (" # ", ConsoleColor.Gray),
                var g => ($"{g.Name}", ConsoleColor.DarkGray)
            };

            ForegroundColor = squareColor;
            Write(squareText);
            ForegroundColor = ConsoleColor.Gray;

            Write("│");
        }
        WriteLine();
    }

    WriteLine($"└{string.Concat(Enumerable.Repeat("───┴", width - 1))}───┘");
    WriteLine();
}

string FormatSquareName(string squareName) =>
    squareName.Length == 2
    ? $"{squareName.First()}0{squareName.Last()}"
    : squareName;

var result = (isLost: false, isNotWon: true);

do {
    Print();

    var isMoveValid = false;
    do {
        WriteLine($"Flagged {grid.Count(g => g.IsFlagged)}/{bombs}");
        Write(">: ");
        isMoveValid = ReadLine()!.Split(' ').Select(s => s.Trim().ToLower()).Where(s => !string.IsNullOrWhiteSpace(s)).ToList() switch {
            [var squareName] when grid.SingleOrDefault(g => g.Name.ToLower() == FormatSquareName(squareName)) is Square s => Expose(s),
            ["f", var squareName] when grid.SingleOrDefault(g => g.Name.ToLower() == FormatSquareName(squareName)) is Square s => s.IsFlagged = !s.IsFlagged,
            _ => false
        };
    }
    while (!isMoveValid);

    result = (grid.Any(g => g.IsExploded), grid.Any(g => !g.IsResolved));
}
while (!result.isLost && result.isNotWon);

Print();

WriteLine(
    result.isLost
    ? "You lost :("
    : "You won!"
);

WriteLine("Press any key to continue...");
ReadKey();

record Square(int X, int Y, bool IsBomb, int AdjacentBombs = 0) {
    public string Name =>
        $"{(char)('A' + Y)}{(X + 1).ToString("D2")}";

    public bool IsResolved =>
        (IsBomb && IsFlagged)
        || (!IsBomb && IsExposed);

    public bool IsExploded =>
        IsBomb && IsExposed;

    public bool IsFlagged { get; set; }

    public bool IsExposed { get; set; }
}
