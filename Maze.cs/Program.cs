using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string filePath = "maze.dat";

        // Kontrola zda soubor existuje
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"Soubor {filePath} neexistuje.");
            WaitForExit();
            return;
        }

        try
        {
            // Načtení souboru
            string[] lines = File.ReadAllLines(filePath);

            // Odstranění bílých znaků na konci řádku
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].TrimEnd();
            }

            // Validace: počet char v řádku pro kontrolu 2D
            int expectedLength = lines[0].Length;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length != expectedLength)
                {
                    Console.WriteLine($"Chyba na řádku {i + 1}: délka {lines[i].Length}, očekává se {expectedLength}.");
                    WaitForExit();
                    return;
                }
            }

            // Vytvoření pole 
            int rows = lines.Length;
            int cols = lines[0].Length;
            char[,] maze = new char[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    maze[i, j] = lines[i][j];
                }
            }

            // Nastavení velikosti konzole
            SetConsoleSize(cols, rows);

            // Najít start a cíl
            var (start, finish) = FindStartAndFinish(maze);

            // Spuštění simulace
            await SimulateDwarfs(maze, start, finish);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"Došlo k chybě: {ex.Message}");
        }
        WaitForExit();
    }

    // Simulace
    static async Task SimulateDwarfs(char[,] maze, Position start, Position finish)
    {
        var dwarfsConfig = new[]
        {
        new { Name = "LeftTurnDwarf", Strategy = (IMovementStrategy)new WallFollowStrategy("left"), Symbol = 'L' },
        //new { Name = "RightTurnDwarf", Strategy = (IMovementStrategy)new WallFollowStrategy("right"), Symbol = 'R' },
        //new { Name = "RandomPortDwarf", Strategy = (IMovementStrategy)new RandomPortStrategy(maze), Symbol = 'T' }
    };

        var dwarfs = new List<(Dwarf Dwarf, string Name, char Symbol, Position PreviousPosition)>
    {
        (new Dwarf(maze, start, finish, dwarfsConfig[0].Strategy), dwarfsConfig[0].Name, dwarfsConfig[0].Symbol, start),
        //(new Dwarf(maze, start, finish, dwarfsConfig[1].Strategy), dwarfsConfig[1].Name, dwarfsConfig[1].Symbol, start),
        //(new Dwarf(maze, start, finish, dwarfsConfig[2].Strategy), dwarfsConfig[2].Name, dwarfsConfig[2].Symbol, start)
    };

        bool allFinished = false;

        // Počáteční vykreslení mapy
        PrintInitialMaze(maze, dwarfs);

        while (!allFinished)
        {
            for (int i = 0; i < dwarfs.Count; i++)
            {
                var dwarf = dwarfs[i];

                if (!dwarf.Dwarf.IsAtFinish())
                {
                    // Smazání starého symbolu
                    Console.SetCursorPosition(dwarf.PreviousPosition.x, dwarf.PreviousPosition.y);
                    Console.Write(maze[dwarf.PreviousPosition.y, dwarf.PreviousPosition.x]);

                    // Pohyb trpaslíka
                    var newPosition = dwarf.Dwarf.Strategy.Move(dwarf.Dwarf.Position, maze);
                    if (IsValidPosition(newPosition, maze))
                    {
                        dwarf.Dwarf.UpdatePosition(newPosition);
                    }
                    else
                    {
                        Console.WriteLine($"Chyba: Neplatná pozice ({newPosition.x}, {newPosition.y})!");
                    }

                    // Aktualizace pozice
                    Console.SetCursorPosition(newPosition.x, newPosition.y);
                    Console.Write(dwarf.Symbol);

                    // Uložení nové předchozí pozice
                    dwarfs[i] = (dwarf.Dwarf, dwarf.Name, dwarf.Symbol, newPosition);
                }
            }

            // Kontrola, zda všichni došli do cíle
            allFinished = dwarfs.TrueForAll(dw => dw.Dwarf.IsAtFinish());

            await Task.Delay(100);
        }

        Console.SetCursorPosition(0, maze.GetLength(0) + 2);
        Console.WriteLine("Všichni trpaslíci úspěšně došli do cíle!");
    }
  
    static void PrintInitialMaze(char[,] maze, List<(Dwarf Dwarf, string Name, char Symbol, Position PreviousPosition)> dwarfs)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        // Vykreslení statického bludiště
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Console.Write(maze[y, x]);
            }
            Console.WriteLine();
        }

        // Vykreslení trpaslíků
        foreach (var dwarf in dwarfs)
        {
            Console.SetCursorPosition(dwarf.Dwarf.Position.x, dwarf.Dwarf.Position.y);
            Console.Write(dwarf.Symbol);
        }
    }

    void SafeSetCursorPosition(int x, int y)
    {
        // Zkontrolujte, zda je pozice v rozsahu
        if (x >= 0 && x < Console.BufferWidth && y >= 0 && y < Console.BufferHeight)
        {
            Console.SetCursorPosition(x, y);
        }
        else
        {
            Console.WriteLine($"Chyba: Nelze nastavit kurzor na ({x}, {y}) - Mimo rozsah konzoly.");
        }
    }


    // Metoda pro nalezení startu a cíle
    public static (Position start, Position finish) FindStartAndFinish(char[,] maze)
    {
        Position start = null;
        Position finish = null;

        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (maze[y, x] == 'S')
                {
                    start = new Position(x, y);
                }
                if (maze[y, x] == 'F')
                {
                    finish = new Position(x, y);
                }

                // Pokud jsme našli oba body, ukončíme hledání
                if (start != null && finish != null)
                {
                    return (start, finish);
                }
            }
        }

        if (start == null || finish == null)
        {
            throw new Exception("Bludišti chybí start (S) nebo cíl (F).");
        }

        return (start, finish);
    }

    private static bool IsValidPosition(Position position, char[,] maze)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        return position.x >= 0 && position.x < cols &&
               position.y >= 0 && position.y < rows;
    }

    interface IMovementStrategy
    {
        Position Move(Position position, char[,] maze);
    }

    // Třída pro uchování pozice
    public class Position
    {
        public int x { get; }
        public int y { get; }

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    // Třída pro strategii WallFollow
    class WallFollowStrategy : IMovementStrategy
    {
        private string wallSide;
        private (int dx, int dy) currentDirection;

        public WallFollowStrategy(string wallSide)
        {
            this.wallSide = wallSide;
            currentDirection = (0, 1); // Defaultní směr: dolů
        }

        public Position Move(Position position, char[,] maze)
        {
            var wallDirection = wallSide == "left"
                ? RotateLeft(currentDirection)
                : RotateRight(currentDirection);

            if (CanMove(wallDirection, position, maze))
            {
                currentDirection = wallDirection;
            }
            else if (!CanMove(currentDirection, position, maze))
            {
                currentDirection = wallSide == "left"
                    ? RotateRight(currentDirection)
                    : RotateLeft(currentDirection);
            }

            int newX = position.x + currentDirection.dx;
            int newY = position.y + currentDirection.dy;

            if (CanMove(currentDirection, position, maze))
            {
                return new Position(newX, newY);
            }

            return position;
        }

        private bool CanMove((int dx, int dy) direction, Position position, char[,] maze)
        {
            int newX = position.x + direction.dx;
            int newY = position.y + direction.dy;

            return newX >= 0 && newY >= 0 && newX < maze.GetLength(1) && newY < maze.GetLength(0) && maze[newY, newX] != '#';
        }

        private (int dx, int dy) RotateLeft((int dx, int dy) direction)
        {
            return (-direction.dy, direction.dx);
        }

        private (int dx, int dy) RotateRight((int dx, int dy) direction)
        {
            return (direction.dy, -direction.dx);
        }
    }

    // Třída pro strategii náhodné teleportace
    class RandomPortStrategy : IMovementStrategy
    {
        private char[,] maze;
        private Position lastPosition;

        public RandomPortStrategy(char[,] maze)
        {
            this.maze = maze; // Inicializace labyrintu
            this.lastPosition = null;
        }

        public Position Move(Position position, char[,] maze)
        {
            var emptyPositions = FindEmptyPositions(maze);

            if (emptyPositions.Count == 0)
            {
                Console.WriteLine("Nebyla nalezena žádná volná pozice pro teleportaci.");
                return position;
            }

            Position newPosition;
            int tries = 0;
            Random rand = new Random();

            do
            {
                int randomIndex = rand.Next(emptyPositions.Count);
                newPosition = emptyPositions[randomIndex];
                tries++;
            }
            while (lastPosition != null &&
                   newPosition.x == lastPosition.x &&
                   newPosition.y == lastPosition.y &&
                   tries < emptyPositions.Count * 2);

            lastPosition = newPosition;
            return newPosition;
        }

        private List<Position> FindEmptyPositions(char[,] maze)
        {
            var emptyPositions = new List<Position>();
            for (int y = 0; y < maze.GetLength(0); y++)
            {
                for (int x = 0; x < maze.GetLength(1); x++)
                {
                    if (maze[y, x] != '#')
                    {
                        emptyPositions.Add(new Position(x, y));
                    }
                }
            }
            return emptyPositions;
        }
    }


    // Třída pro trpaslíka
    class Dwarf
    {
        public char[,] Maze { get; }
        public Position Position { get; private set; }
        public Position Finish { get; }
        public IMovementStrategy Strategy { get; }

        public Dwarf(char[,] maze, Position start, Position finish, IMovementStrategy strategy)
        {
            Maze = maze;
            Position = start;
            Finish = finish;
            Strategy = strategy;
        }

        public void Move()
        {
            Position = Strategy.Move(Position, Maze);
        }

        public void UpdatePosition(Position newPosition)
        {
            Position = newPosition;
        }

        public bool IsAtFinish()
        {
            return Position.x == Finish.x && Position.y == Finish.y;
        }
    }


    // Nastavení velikosti konzole
    static void SetConsoleSize(int cols, int rows)
    {
        try
        {
            // Nastavení vyrovnávací paměti na velikost větší nebo rovnou oknu
            int bufferWidth = Math.Max(cols + 2, Console.BufferWidth);
            int bufferHeight = Math.Max(rows + 2, Console.BufferHeight);

            Console.SetBufferSize(bufferWidth, bufferHeight);

            // Nastavení velikosti okna na maximum dostupné velikosti konzoly
            int windowWidth = Math.Min(cols + 2, Console.LargestWindowWidth);
            int windowHeight = Math.Min(rows + 2, Console.LargestWindowHeight);

            Console.SetWindowSize(windowWidth, windowHeight);

            Console.WriteLine($"Konzole nastavena: Šířka = {windowWidth}, Výška = {windowHeight}, Vyrovnávací paměť = ({bufferWidth}, {bufferHeight})");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při nastavování konzoly: {ex.Message}");
        }
    }




    // Potvrzení o ukončení programu
    static void WaitForExit()
    {
        Console.WriteLine("\nStiskni libovolnou klávesu pro ukončení programu...");
        Console.ReadKey();
    }
}
