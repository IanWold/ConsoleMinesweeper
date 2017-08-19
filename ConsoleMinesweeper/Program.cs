using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleMinesweeper
{
	class Program
	{
		static GridSpace[,] Minefield;

		static int size;
		static int bombs;

		static void Main(string[] args)
		{
			Console.BackgroundColor = ConsoleColor.DarkRed;
			Console.ForegroundColor = ConsoleColor.White;

			while (true)
			{
				Console.Clear();
				Console.WriteLine("Welcome to Console Minesweeper! Whoa.");

				GetValues();
				ConstructGrid();

				Console.BackgroundColor = ConsoleColor.Gray;
				Console.ForegroundColor = ConsoleColor.Black;

				bool? isWin = false;

				while (true)
				{
					WriteField();

					if (CheckWin())
					{
						isWin = true;
						break;
					}

					Console.Write(">: ");

					var input = Console.ReadLine().ToLower();
					Console.WriteLine();

					try
					{
						if (input == "exit")
						{
							isWin = null;
							break;
						}
						else
						{
							var splinput = input.Split(' ');

							if (splinput.Length == 2 && splinput[0] == "f")
							{ //flag
								var location = GetLocation(splinput[1]);

								if (!Minefield[location.Row, location.Column].IsHidden)
								{
									WriteLineColor("Cannot flag a pressed space.", ConsoleColor.Red);
								}
								else
								{
									Minefield[location.Row, location.Column].IsFlagged = !Minefield[location.Row, location.Column].IsFlagged;
								}
							}
							else
							{ //press
								var location = GetLocation(splinput[0]);

								if (!Minefield[location.Row, location.Column].IsHidden)
								{
									WriteLineColor("Cannot press an already-pressed space.", ConsoleColor.Red);
								}
								else
								{
									Minefield[location.Row, location.Column].IsHidden = false;

									if (Minefield[location.Row, location.Column].IsBomb) //Oh shit
									{
										RevealAllBombs();
										WriteField();
										isWin = false;
										break;
									}
									else //Whew
									{
										PressSpace(location);
									}
								}
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("Try again...");
					}
				}

				if (isWin.HasValue) EndGame(isWin.Value);

				Console.BackgroundColor = ConsoleColor.DarkRed;
				Console.ForegroundColor = ConsoleColor.White;
				Console.Clear();

				Console.Write("PLAY AGAIN (Y/N)? ");
				var yn = Console.ReadLine();

				if (yn != "y") break;
			}
		}

		static void GetValues()
		{
			size = 0;
			do
			{
				Console.Write("Size of field: ");
				size = Convert.ToInt32(Console.ReadLine());
				if (size > 26) Console.WriteLine("Field size must be between 1 and 26, including.");
			} while (size > 26);

			bombs = 0;
			do
			{
				Console.Write("Number of bombs: ");
				bombs = Convert.ToInt32(Console.ReadLine());
				if (bombs > size * size) Console.WriteLine("Field size must be between 1 and " + (size * size) + ", including.");
			} while (bombs > size * size);
		}

		static void ConstructGrid()
		{
			Minefield = new GridSpace[size, size];
			var rnd = new Random();

			var allGridSpaces = new List<GridSpace>();
			for (int i = 0; i < size * size; i++)
			{
				allGridSpaces.Add(new GridSpace(i < bombs));
			}

			while (allGridSpaces.Count > 0)
			{
				var col = rnd.Next(0, size);
				var row = rnd.Next(0, size);

				var loc = (row = rnd.Next(0, size), col = rnd.Next(0, size));

				if (Minefield[row, col] != null)
				{
					var shouldBreak = false;
					for (row = 0; row < size; row++)
					{
						for (col = 0; col < size; col++)
						{
							if (Minefield[row, col] == null) shouldBreak = true;
							if (shouldBreak) break;
						}
						if (shouldBreak) break;
					}
				}

				Minefield[row, col] = allGridSpaces[0];
				Minefield[row, col].Location = (row, col);
				allGridSpaces.RemoveAt(0);
			}
		}

		static void WriteField()
		{
			Console.Clear();

			//Write numbers
			Console.Write("  ");
			for (int i = 0; i < size; i++)
			{
				Console.Write("  " + i + (i < 10 ? " " : ""));
			}

			Console.WriteLine();

			//Write each row
			for (int r = 0; r < size; r++)
			{
				Console.Write("  ");
				WriteHorizontalLine();

				Console.Write((char)(r + 65) + " ");
				
				for (int c = 0; c < size; c++)
				{
					Console.Write("|");
					var toColor = ConsoleColor.Black;

					if (Minefield[r, c].ShowBombThroughFlag && Minefield[r, c].IsFlaggedCorrectly) toColor = ConsoleColor.Green;
					else if (Minefield[r, c].IsFlagged) toColor = ConsoleColor.Green;
					else if (Minefield[r, c].IsHidden) toColor = ConsoleColor.Black;
					else if (Minefield[r, c].IsBomb) toColor = ConsoleColor.Red;
					else
					{
						switch (Convert.ToInt32(Minefield[r, c].Value.Trim(' ')))
						{
							case 0:
								toColor = ConsoleColor.Gray;
								break;

							case 1:
								toColor = ConsoleColor.DarkRed;
								break;

							case 2:
								toColor = ConsoleColor.Blue;
								break;

							case 3:
								toColor = ConsoleColor.Magenta;
								break;

							case 4:
								toColor = ConsoleColor.DarkBlue;
								break;

							case 5:
								toColor = ConsoleColor.Cyan;
								break;

							case 6:
								toColor = ConsoleColor.DarkMagenta;
								break;

							case 7:
								toColor = ConsoleColor.DarkCyan;
								break;

							case 8:
								toColor = ConsoleColor.DarkGray;
								break;
						}
					}

					WriteColor(Minefield[r, c].ToString(), toColor);
				}

				Console.WriteLine("|");
			}

			//Write last horizontal line
			Console.Write("  ");
			WriteHorizontalLine();

			Console.WriteLine();
		}

		static void WriteHorizontalLine()
		{
			for (int i = 0; i < size; i++)
			{
				Console.Write("+---");
			}

			Console.WriteLine("+");
		}

		static void PressSpace((int Row, int Column) location)
		{
			Minefield[location.Row, location.Column].IsHidden = false;

			var bombCount = 0;
			var surroundingSpaces = GetSurroundingSpaces(location);
			foreach (var space in surroundingSpaces)
			{
				if (Minefield[location.Row + space.Row, location.Column + space.Column].IsBomb) bombCount++;
			}

			if (bombCount == 0)
			{
				foreach (var space in surroundingSpaces)
				{
					if (Minefield[location.Row + space.Row, location.Column + space.Column].IsHidden) PressSpace((location.Row + space.Row, location.Column + space.Column));
				}
			}

			Minefield[location.Row, location.Column].Value = " " + bombCount.ToString() + " ";
		}

		static List<(int Row, int Column)> GetSurroundingSpaces((int Row, int Column) location)
		{
			var toReturn = new List<(int, int)>();

			for (var r = -1; r <= 1; r++)
			{
				for (var c = -1; c <= 1; c++)
				{
					if (!(r == 0 && c == 0) &&
						location.Row + r >= 0 &&
						location.Row + r < size &&
						location.Column + c >= 0 &&
						location.Column + c < size)
						toReturn.Add((r, c));
				}
			}

			return toReturn;
		}

		static void RevealAllBombs()
		{
			foreach (var s in Minefield)
			{
				if (s.IsBomb)
				{
					s.ShowBombThroughFlag = true;
					s.IsHidden = false;
				}
			}


		}

		static bool CheckWin()
		{
			foreach (var space in Minefield)
			{
				if (!(space.IsFlaggedCorrectly || !space.IsHidden)) return false;
			}

			return true;
		}

		static void EndGame(bool isWin)
		{
			Console.WriteLine("Game over. You're a " + (isWin ? "winner!" : "loser."));
			Console.ReadKey();
		}

		static (int Row, int Column) GetLocation(string location) =>
			(char.ToUpper(location.ToCharArray()[0]) - 65, Convert.ToInt32(location.Substring(1, location.Length - 1)));

		static void WriteColor(string toWrite, ConsoleColor toColor)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = toColor;
			Console.Write(toWrite);
			Console.ForegroundColor = color;
		}

		static void WriteLineColor(string toWrite, ConsoleColor toColor) => WriteColor(toWrite + "\r\n", toColor);
	}

	class GridSpace
	{
		public (int Row, int Column) Location;

		public bool IsHidden;

		public bool IsBomb;

		public bool IsFlagged;

		public bool ShowBombThroughFlag;

		public string Value = "   ";

		public bool IsFlaggedCorrectly
		{
			get { return IsFlagged && IsBomb; }
		}

		public GridSpace(bool isBomb)
		{
			IsHidden = true;
			IsFlagged = false;
			ShowBombThroughFlag = false;

			IsBomb = isBomb;
		}

		public override string ToString()
		{
			if (IsFlaggedCorrectly && ShowBombThroughFlag) return " @ ";
			if (IsFlagged) return " # ";
			else if (IsHidden) return (char)(Location.Row + 65) + Location.Column.ToString() + (Location.Column < 10 ? " " : "");
			else if (IsBomb) return " @ ";
			else return Value;
		}
	}
}
