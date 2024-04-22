using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
namespace Minesweeper
{
    class Program
    {
        static char[,] Board = new char[1, 1];
        static int Height;
        static int Width;
        static int BombAmount;
        static int FlagAmount;
        static char BombChar = 'B';
        static char FlagChar = 'F';
        static char BoxChar = ' ';
        static string PressKey = "Enter";
        static int X = 0;
        static int Y = 0;
        static List<(int, int)> BombCoords = new List<(int, int)>();
        static List<(int, int)> PressedCoords = new List<(int, int)>();
        static Random Random = new Random();

        static Stopwatch Timer = new Stopwatch();
        static long TimerCurrent;
        static bool IsDead = false;
        static bool DidWin = false;
        static int Minutes;

        static int FlagsCorrect;

        static int TilesPressed;
        
        static void Main(string[] args)
        {
            while (true)
            {
                MainMeny();
                MainSwitch(ValidInput(1, 5)); //Initialize and Draw boards
            }
        }
        static void MainSwitch(int ans)
        {
            Console.Clear();
            switch (ans)
            {
                case 1: //Easy
                    BombAmount = 15;
                    Width = 11;
                    Height = 11;
                    InitializeBoard(Width, Height);
                    RandomBombs(Width, Height);
                    DrawBoard();
                    while (true)
                    {
                        CheckKey();
                    }

                case 2: //Medium
                    BombAmount = 50;
                    Width = 17;
                    Height = 17;
                    InitializeBoard(Width, Height);
                    RandomBombs(Width, Height);
                    DrawBoard();
                    while (true)
                    {
                        CheckKey();
                    }
                case 3: //Hard
                    BombAmount = 150;
                    Width = 24;
                    Height = 24;
                    InitializeBoard(Width, Height);
                    RandomBombs(Width, Height);
                    DrawBoard();
                    while (true)
                    {
                        CheckKey();
                    }
                case 4: //Custom
                    Console.Write("Width? (min 2, max 40): ");
                    Width = ValidInput(2, 40);
                    Console.Write("Height? (min 2, max 40): ");
                    Height = ValidInput(2, 40);
                    Console.Write($"Amount of bombs? (max {(Width * Height) - 1}): ");
                    BombAmount = ValidInput(1, (Width * Height) - 1);
                    Console.Clear();
                    InitializeBoard(Width, Height);
                    RandomBombs(Width, Height);
                    DrawBoard();
                    //CW at the top that keeps track of amount of flags, squares and time
                    while (true)
                    {
                        CheckKey();
                    }
                case 5: //Rules
                    Console.CursorVisible = false;
                    Console.WriteLine($"Navigate with WASD or Arrow keys.");
                    Console.WriteLine($"Press Space or {PressKey} to click a square.");
                    Console.WriteLine("If you click on a bomb you die.");
                    Console.WriteLine();
                    Console.WriteLine("Flags are used to mark bombs but are not necessary to win.");
                    Console.WriteLine($"Press {FlagChar} to put down a flag.");
                    Console.WriteLine($"Press {FlagChar} on a flag to pick it up.");
                    Console.WriteLine();
                    Console.WriteLine("Once you have died/won, the tiles will be revealed and the Flags will turn Purple/Cyan.");
                    Console.WriteLine("Green tiles are tiles you pressed, black tiles with a white digit are tiles you didn't press.");
                    Console.WriteLine();
                    Console.WriteLine("If the flag is Cyan, that means you placed it on a bomb (good).");
                    Console.WriteLine("If the flag is Purple, that means you put your flag down on a clear tile (bad).");
                    Console.WriteLine("Light Red bomb is the bomb you died to.");
                    Console.WriteLine();
                    Console.WriteLine("Goal: Press all tiles that are not bombs.");
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    Console.Clear();
                    Console.CursorVisible = true;
                    break;
            }
        }
        static char CheckAdjacent()
        {
            int adjacentBombs = 0;

            // Check adjacent squares
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = X + dx;
                    int ny = Y + dy;

                    // Skip the current square
                    if (dx == 0 && dy == 0)
                    {
                        continue;
                    }

                    // Check if the adjacent square is within the bounds of the board
                    if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                    {
                        foreach (var item in BombCoords)
                        {
                            if (item.Item1 == nx && item.Item2 == ny)
                            {
                                adjacentBombs++;
                            }
                        }
                    }
                }
            }

            // Convert the count of adjacent bombs to a character
            string a = adjacentBombs.ToString();
            char adjacentBombsChar = Convert.ToChar(a);
            return adjacentBombsChar;
        }

        static void CheckAdjacentAny()
        {
            

            for (int y = 0; y < Board.GetLength(0); y++)
            {
                for (int x = 0; x < Board.GetLength(1); x++)
                {
                    int adjacentBombs = 0;
                    // Check adjacent squares
                    for (int dx = -1; dx <= 1; dx++)
                    {
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int nx = x + dx;
                            int ny = y + dy;

                            // Skip the current square
                            if (dx == 0 && dy == 0)
                            {
                                continue;
                            }

                            // Check if the adjacent square is within the bounds of the board
                            if (nx >= 0 && nx < Width && ny >= 0 && ny < Height)
                            {
                                foreach (var item in BombCoords)
                                {
                                    if (item.Item1 == nx && item.Item2 == ny)
                                    {
                                        adjacentBombs++;
                                    }
                                }
                            }
                        }
                    }
                    string a = adjacentBombs.ToString();
                    char adjacentBombsChar = Convert.ToChar(a);
                    if (Board[x, y] != FlagChar)
                    {
                        Board[x, y] = adjacentBombsChar;
                    }
                }
            }
        }
        static void TimerUpdater()
        {
            if (TimerCurrent < Timer.ElapsedMilliseconds/1000)
            {
                TimerCurrent = Timer.ElapsedMilliseconds/1000;
                Console.Clear();
                DrawBoard();
            }
        }
        static void InitializeBoard(int width, int height)
        {
            Timer.Restart();
            Minutes = 0;
            FlagsCorrect = 0;
            PressedCoords = new List<(int, int)>();
            Board = new char[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Board[x, y] = BoxChar;
                }
            }
            Timer.Start();
            TimerCurrent = Timer.ElapsedMilliseconds / 1000;
            FlagAmount = BombAmount;
            TilesPressed = 0;
        }
        static void CheckIfBombOnly()
        {
            if (!IsDead && TilesPressed == (Height * Width) - BombAmount)
            {
                DidWin = true;
            }
        }
        static void DrawBoard()
        {
            CheckIfBombOnly();
            Console.ForegroundColor = ConsoleColor.White;
            if (IsDead || DidWin)
            {
                Timer.Stop();
            }
            if (Timer.ElapsedMilliseconds / 1000 >= 60)
            {
                Minutes += 1;
                Timer.Restart();
                TimerCurrent = Timer.ElapsedMilliseconds / 1000;
                Console.WriteLine($"Flags left: {FlagAmount}    Bomb total: {BombAmount}    Timer: {Minutes}m {TimerCurrent}s");
            }
            else if (Minutes > 0)
            {
                Console.WriteLine($"Flags left: {FlagAmount}    Bomb total: {BombAmount}    Timer: {Minutes}m {TimerCurrent}s");
            }
            else
            {
                Console.WriteLine($"Flags left: {FlagAmount}    Bomb total: {BombAmount}    Timer: {TimerCurrent}s");
            }
            
            Console.WriteLine();
            for (int y = 0; y < Board.GetLength(1); y++)
            {
                for (int x = 0; x < Board.GetLength(0); x++)
                {
                    
                    if (IsDead || DidWin)
                    {
                        foreach (var item in PressedCoords)
                        {
                            if (x == item.Item1 && y == item.Item2)
                            {
                                Console.BackgroundColor = ConsoleColor.DarkGreen;
                                Console.ForegroundColor = ConsoleColor.Black;
                                
                            }
                        }
                        if (Board[x, y] == FlagChar)
                        {
                            Console.BackgroundColor = ConsoleColor.DarkMagenta;
                            Console.ForegroundColor = ConsoleColor.Black;
                        }
                        foreach (var item in BombCoords)
                        {
                            if (item.Item1 == x)
                            {
                                if (item.Item2 == y)
                                {
                                    if (Board[x, y] == FlagChar)
                                    {
                                        Console.BackgroundColor = ConsoleColor.Cyan;
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        FlagsCorrect++;
                                    }
                                    else if (item.Item1 == X && item.Item2 == Y)
                                    {
                                        Console.BackgroundColor = ConsoleColor.Red;
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        Board[x, y] = BombChar;
                                    }
                                    else
                                    {
                                        Console.BackgroundColor = ConsoleColor.DarkRed;
                                        Console.ForegroundColor = ConsoleColor.Black;
                                        Board[x, y] = BombChar;
                                    }
                                }
                            }
                        }
                        
                    }
                    else if (Board[x, y] == FlagChar)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Blue;
                        if (x == X && y == Y)
                        {
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.White;
                        }
                    }
                    else if (x == X && y == Y)
                    {
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.BackgroundColor = ConsoleColor.White;
                    }
                    
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.BackgroundColor = ConsoleColor.Black;
                    }
                    
                    
                    Console.Write($"|{Board[x, y]}|");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.WriteLine();
            }
            Console.CursorVisible = false;
            if (IsDead)
            {
                Console.WriteLine();
                Console.WriteLine("You lost!");
                Console.WriteLine();
                Console.WriteLine($"Non-bomb tiles pressed: {TilesPressed} / {(Board.GetLength(0) * Board.GetLength(1))-BombAmount}");
                Console.WriteLine();
                Console.WriteLine($"Flags placed: {BombAmount - FlagAmount} / {BombAmount}");
                Console.WriteLine($"Flags correct: {FlagsCorrect} / {BombAmount - FlagAmount}");
                Console.WriteLine($"Flags wrong: {BombAmount-FlagAmount-FlagsCorrect} / {BombAmount - FlagAmount}");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.ReadKey();
                Console.ReadKey();
                Console.ReadKey();
                Console.Clear();
                IsDead = false;
                BombCoords = new List<(int, int)>();
                X = 0;
                Y = 0;
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorVisible = true;
                MainMeny();
                MainSwitch(ValidInput(1, 5));
            }
            if (DidWin)
            {
                Console.WriteLine();
                Console.WriteLine("Yippee!");
                Console.WriteLine();
                Console.WriteLine($"Non-bomb tiles pressed: {TilesPressed} / {(Board.GetLength(0) * Board.GetLength(1)) - BombAmount}");
                Console.WriteLine();
                Console.WriteLine($"Flags placed: {BombAmount - FlagAmount} / {BombAmount}");
                Console.WriteLine($"Flags correct: {FlagsCorrect} / {BombAmount - FlagAmount}");
                Console.WriteLine($"Flags wrong: {BombAmount - FlagAmount - FlagsCorrect} / {BombAmount - FlagAmount}");
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ForegroundColor = ConsoleColor.Black;
                Console.ReadKey();
                Console.ReadKey();
                Console.ReadKey();
                Console.Clear();
                DidWin = false;
                BombCoords = new List<(int, int)>();
                X = 0;
                Y = 0;
                Console.ForegroundColor = ConsoleColor.White;
                Console.CursorVisible = true;
                MainMeny();
                MainSwitch(ValidInput(1, 5));
            }
            Console.ForegroundColor = ConsoleColor.Black;
        }
        static void MainMeny()
        {
            Console.CursorVisible = true;
            Console.WriteLine("Select difficulty");
            Console.WriteLine("1. Easy (11x11, 15 bombs)"); //DISPLAY AMOUNT OF BOMBS
            Console.WriteLine("2. Medium (17x17, 50 bombs)");
            Console.WriteLine("3. Hard (24x24, 150 bombs)");
            Console.WriteLine("4. Custom");
            Console.WriteLine("5. How to play");
            Console.WriteLine();
            Console.Write("Your input: ");
        }
        static void RandomBombs(int width, int height)
        {
            for (int i = 0; i < BombAmount; i++)
            {
                int XCoord = Random.Next(width);
                int YCoord = Random.Next(height);
                if (!BombCoords.Contains((XCoord, YCoord)))
                {
                    BombCoords.Add((XCoord, YCoord));
                }
                else
                {
                    i--;
                }
            }

        }
        static void CheckKey()
        {
            //check what key is pressed
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo Key = Console.ReadKey();
                switch (Key.Key)
                {
                    case ConsoleKey.Enter:
                        if (Board[X, Y] == 'F')
                        {
                            break;
                        }
                        foreach (var item in BombCoords)
                        {
                            if(item.Item1 == X)
                            {
                                if (item.Item2 == Y)
                                {
                                    IsDead = true;
                                    
                                }
                            }
                        }
                        if (!IsDead)
                        {
                            Board[X, Y] = CheckAdjacent();
                            PressedCoords.Add((X, Y));
                            TilesPressed++;
                        }
                        else
                        {
                            CheckAdjacentAny();
                        }

                        Console.Clear(); //These are just so board doesn't update on any key but only on specific keys
                        DrawBoard();
                        break;
                    case ConsoleKey.Spacebar:
                        if (Board[X, Y] == 'F')
                        {
                            break;
                        }
                        foreach (var item in BombCoords)
                        {
                            if (item.Item1 == X)
                            {
                                if (item.Item2 == Y)
                                {
                                    IsDead = true;

                                }
                            }
                        }
                        if (!IsDead)
                        {
                            Board[X, Y] = CheckAdjacent();
                            PressedCoords.Add((X, Y));
                            TilesPressed++;
                        }
                        else
                        {
                            CheckAdjacentAny();
                        }

                        Console.Clear(); //These are just so board doesn't update on any key but only on specific keys
                        DrawBoard();
                        break;
                    case ConsoleKey.F:
                        if (Board[X, Y] == BoxChar && FlagAmount != 0)
                        {
                            Board[X, Y] = FlagChar;
                            FlagAmount -= 1;
                        }
                        else if (Board[X, Y] == FlagChar)
                        {
                            FlagAmount += 1;
                            Board[X, Y] = BoxChar;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.RightArrow:
                        if (X < Width-1)
                        {
                            X += 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.D:
                        if (X < Width - 1)
                        {
                            X += 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.LeftArrow:
                        if (X > 0)
                        {
                            X -= 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.A:
                        if (X > 0)
                        {
                            X -= 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.UpArrow:
                        if (Y > 0)
                        {
                            Y -= 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.W:
                        if (Y > 0)
                        {
                            Y -= 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.DownArrow:
                        if (Y < Height-1)
                        {
                            Y += 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                    case ConsoleKey.S:
                        if (Y < Height - 1)
                        {
                            Y += 1;
                        }
                        Console.Clear();
                        DrawBoard();
                        break;
                }
            }
            TimerUpdater();
        }
        static int ValidInput(int min, int max)
        {
            bool a = int.TryParse(Console.ReadLine(), out int n);
            while (!a || n < min || n > max)
            {
                Console.Write($"Invalid input, type a digit between {min} and {max}: ");
                a = int.TryParse(Console.ReadLine(), out n);
            }
            return n;
        }
    }
}