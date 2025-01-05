﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

class Program
{
    // Odkaz souboru Maze
    private static string filePath = "maze.dat";

    // Main
    static async Task Main(string[] args)
    {
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

            // Nalezení startu a cíle
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

    // Simulace trpaslíků
    static async Task SimulateDwarfs(char[,] maze, Position start, Position finish)
    {
        // Továrna na trpaslíky 
        var dwarfFactory = new DwarfFactory(maze, start, finish);

        // Nastavení trpaslíků
        var dwarfConfigs = new[]
        {
        new { Name = "LeftTurnDwarf", Symbol = 'L' },
        new { Name = "RightTurnDwarf", Symbol = 'R' },
        new { Name = "RandomPortDwarf", Symbol = 'T' },
        new { Name = "PathFollowingDwarf", Symbol = 'P' }
    };

        // Začátek prvního trpaslíka
        var dwarfs = new List<(Dwarf Dwarf, string Name, char Symbol, Position PreviousPosition)>
    {
        (dwarfFactory.CreateDwarf(dwarfConfigs[0].Name), dwarfConfigs[0].Name, dwarfConfigs[0].Symbol, start)
    };

        // Vykreslení bludiště s trpaslíkem
        PrintInitialMaze(maze, dwarfs);

        bool allFinished = false;
        int currentDwarfIndex = 1;
        var dwarfAddTimers = new List<int> { 0, 5000, 10000, 15000 }; // po 5s 
        var startTime = DateTime.Now;

        while (!allFinished)
        {
            // Výpočet času od začátku simulace
            var elapsedTime = (DateTime.Now - startTime).TotalMilliseconds;

            // Přidání trpaslíka dle času v dwarfAddTimers
            if (currentDwarfIndex < dwarfConfigs.Length && elapsedTime >= dwarfAddTimers[currentDwarfIndex])
            {
                var newDwarf = dwarfFactory.CreateDwarf(dwarfConfigs[currentDwarfIndex].Name);
                dwarfs.Add((newDwarf, dwarfConfigs[currentDwarfIndex].Name, dwarfConfigs[currentDwarfIndex].Symbol, start));

                SafeSetCursorPosition(start.x, start.y);
                Console.Write(dwarfConfigs[currentDwarfIndex].Symbol);

                currentDwarfIndex++;
            }

            for (int i = 0; i < dwarfs.Count; i++)
            {
                var dwarf = dwarfs[i];

                if (!dwarf.Dwarf.IsAtFinish())
                {
                    SafeSetCursorPosition(dwarf.PreviousPosition.x, dwarf.PreviousPosition.y);
                    Console.Write(maze[dwarf.PreviousPosition.y, dwarf.PreviousPosition.x]);

                    var newPosition = dwarf.Dwarf.Strategy.Move(dwarf.Dwarf.Position, maze);
                    if (IsValidPosition(newPosition, maze))
                    {
                        dwarf.Dwarf.UpdatePosition(newPosition);
                    }

                    SafeSetCursorPosition(newPosition.x, newPosition.y);
                    Console.Write(dwarf.Symbol);

                    dwarfs[i] = (dwarf.Dwarf, dwarf.Name, dwarf.Symbol, newPosition);
                }
            }

            DisplayDwarfPositions(dwarfs, maze.GetLength(0));
            allFinished = dwarfs.TrueForAll(dw => dw.Dwarf.IsAtFinish());

            // Rychlost pohybu trpaslíka
            await Task.Delay(100);
        }

        Console.SetCursorPosition(0, maze.GetLength(0) + dwarfs.Count + 2);
        Console.WriteLine("Všichni trpaslíci úspěšně došli do cíle!");
    }

    // Zobrazení aktuální pozice trpaslíků
    static void DisplayDwarfPositions(List<(Dwarf Dwarf, string Name, char Symbol, Position PreviousPosition)> dwarfs, int mazeHeight)
    {
        // Umístění zobrazení pod bludiště
        Console.SetCursorPosition(0, mazeHeight + 1);

        // Nadpis a výpis každého trpaslíka
        Console.WriteLine("Aktuální pozice trpaslíků:");
        foreach (var dwarf in dwarfs)
        {
            var pos = dwarf.Dwarf.Position;
            string status = dwarf.Dwarf.IsAtFinish() ? "Dorazil do cíle" : $"({pos.x}, {pos.y})";
            Console.WriteLine($" - {dwarf.Name} ({dwarf.Symbol}): {status}");
        }
    }

    // Vykreslední bludiště a trpaslíků
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

    // Bezpečné nastavení pozice 
    static void SafeSetCursorPosition(int x, int y)
    {
        // Zkontrolujte, zda je pozice v rozsahu
        if (x >= 0 && x < Console.BufferWidth && y >= 0 && y < Console.BufferHeight)
        {
            Console.SetCursorPosition(x, y);
        }
        else
        {
            Console.WriteLine($"Neplatná pozice kurzoru: ({x}, {y}).");
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

    // Metoda pro kontrolu platnosti pozice
    private static bool IsValidPosition(Position position, char[,] maze)
    {
        int rows = maze.GetLength(0);
        int cols = maze.GetLength(1);

        return position.x >= 0 && position.x < cols &&
               position.y >= 0 && position.y < rows;
    }

    // Rozhraní pro strategii pohybu
    interface IMovementStrategy
    {
        Position Move(Position position, char[,] maze);
    }

    // Třída pro strategii WallFollow
    class WallFollowStrategy : IMovementStrategy
    {
        private string wallSide;
        private (int dx, int dy) currentDirection;

        public WallFollowStrategy(string wallSide)
        {
            this.wallSide = wallSide;
            this.currentDirection = (0, 1); // Defaultní směr: dolů
        }

        public Position Move(Position position, char[,] maze)
        {
            var wallDirection = wallSide == "left"
                ? RotateLeft(currentDirection)
                : RotateRight(currentDirection);

            if (CanMove(wallDirection, position, maze))
            {
                // Pokud je možné se pohnout ve směru zdi, změní směr na zeď
                currentDirection = wallDirection;
            }
            else if (CanMove(currentDirection, position, maze))
            {
                // Pokud není možné se pohnout směrem zdi, ale směr vpřed je volný, zůstane v aktuálním směru
            }
            else
            {
                // Pokud nelze jít ani na stranu ani vpřed, otočí se na opačnou stranu
                currentDirection = wallSide == "left"
                    ? RotateRight(currentDirection)
                    : RotateLeft(currentDirection);
            }

            // Vypočítá novou pozici
            int newX = position.x + currentDirection.dx;
            int newY = position.y + currentDirection.dy;

            // Pokud je pohyb možný, vrátí novou pozici, jinak zůstává na místě
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

            // Kontrola, zda je cílová pozice v rámci bludiště a není to zeď
            return newX >= 0 && newY >= 0 &&
                   newX < maze.GetLength(1) &&
                   newY < maze.GetLength(0) &&
                   maze[newY, newX] != '#';
        }

        private (int dx, int dy) RotateLeft((int dx, int dy) direction)
        {
            return (direction.dy, -direction.dx);
        }

        private (int dx, int dy) RotateRight((int dx, int dy) direction)
        {
            return (-direction.dy, direction.dx);
        }
    }

    // Třída pro strategii náhodné teleportace
    class RandomPortStrategy : IMovementStrategy
    {
        private List<Position> emptyPositions;
        private Position lastPosition;

        public RandomPortStrategy(char[,] maze)
        {
            // Najdeme všechna volná políčka včetně cíle F
            emptyPositions = FindEmptyPositions(maze);
            lastPosition = null;
        }

        public Position Move(Position position, char[,] maze)
        {
            if (emptyPositions.Count == 0)
            {
                Console.WriteLine("Všechna volná políčka již byla navštívena.");
                return position;
            }

            Position newPosition;
            Random rand = new Random();

            do
            {
                int randomIndex = rand.Next(emptyPositions.Count);
                newPosition = emptyPositions[randomIndex];
            }
            while (lastPosition != null &&
                   newPosition.x == lastPosition.x &&
                   newPosition.y == lastPosition.y &&
                   emptyPositions.Count > 1);

            // Odstraníme navštívenou pozici ze seznamu
            emptyPositions.Remove(newPosition);
            lastPosition = newPosition;

            return newPosition;
        }

        private List<Position> FindEmptyPositions(char[,] maze)
        {
            var positions = new List<Position>();
            for (int y = 0; y < maze.GetLength(0); y++)
            {
                for (int x = 0; x < maze.GetLength(1); x++)
                {
                    if (maze[y, x] != '#')
                    {
                        positions.Add(new Position(x, y));
                    }
                }
            }
            return positions;
        }
    }

    // Třída pro strategii následování cesty
    class PathFollowingStrategy : IMovementStrategy
    {
        private List<Position> path;
        private int currentStep;

        public PathFollowingStrategy(char[,] maze, Position start, Position finish)
        {
            path = FindPath(maze, start, finish);
            currentStep = 0;
        }

        private List<Position> FindPath(char[,] maze, Position start, Position finish)
        {
            var directions = new (int dx, int dy)[]
            {
        (0, -1),  // Nahoru
        (-1, 0),  // Vlevo
        (0, 1),   // Dolů
        (1, 0)    // Vpravo
            };

            int rows = maze.GetLength(0);
            int cols = maze.GetLength(1);

            var visited = new bool[rows, cols];
            var queue = new Queue<List<Position>>();

            // Inicializace BFS
            queue.Enqueue(new List<Position> { start });
            visited[start.y, start.x] = true;

            while (queue.Count > 0)
            {
                var path = queue.Dequeue();
                var current = path[path.Count - 1];

                // Pokud jsme dorazili na cíl, vracíme cestu
                if (current.x == finish.x && current.y == finish.y)
                {
                    return path;
                }

                // Prozkoumání sousedních pozic
                foreach (var (dx, dy) in directions)
                {
                    int newX = current.x + dx;
                    int newY = current.y + dy;

                    if (newX >= 0 && newY >= 0 && newX < cols && newY < rows &&
                        maze[newY, newX] != '#' && !visited[newY, newX])
                    {
                        visited[newY, newX] = true;
                        var newPath = new List<Position>(path) { new Position(newX, newY) };
                        queue.Enqueue(newPath);
                    }
                }
            }

            throw new Exception("Cesta nenalezena.");
        }


        public Position Move(Position position, char[,] maze)
        {
            if (currentStep < path.Count)
            {
                return path[currentStep++];
            }
            return position; // Zůstane na místě, pokud dosáhl konce cesty
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

    // Třída pro vytváření trpaslíků pomocí Factory Pattern
    class DwarfFactory
    {
        private char[,] _maze;
        private Position _start;
        private Position _finish;

        public DwarfFactory(char[,] maze, Position start, Position finish)
        {
            _maze = maze;
            _start = start;
            _finish = finish;
        }

        public Dwarf CreateDwarf(string name)
        {
            switch (name)
            {
                case "LeftTurnDwarf":
                    return new Dwarf(_maze, _start, _finish, new WallFollowStrategy("left"));

                case "RightTurnDwarf":
                    return new Dwarf(_maze, _start, _finish, new WallFollowStrategy("right"));

                case "RandomPortDwarf":
                    return new Dwarf(_maze, _start, _finish, new RandomPortStrategy(_maze));

                case "PathFollowingDwarf":
                    return new Dwarf(_maze, _start, _finish, new PathFollowingStrategy(_maze, _start, _finish));

                default:
                    throw new ArgumentException($"Unknown dwarf type: {name}");
            }
        }
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

    // Nastavení velikosti konzole
    static void SetConsoleSize(int cols, int rows)
    {
        try
        {
            int bufferWidth = Math.Max(cols + 2, Console.BufferWidth);
            int bufferHeight = Math.Max(rows + 10, Console.BufferHeight); // Rezerva pro pozice pod bludištěm

            Console.SetBufferSize(bufferWidth, bufferHeight);

            int windowWidth = Math.Min(bufferWidth, Console.LargestWindowWidth);
            int windowHeight = Math.Min(bufferHeight, Console.LargestWindowHeight);

            Console.SetWindowSize(windowWidth, windowHeight);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba při nastavování velikosti konzole: {ex.Message}");
        }
    }

    // Potvrzení o ukončení programu
    static void WaitForExit()
    {
        Console.WriteLine("\nStisknutím klávesy ukončíš Maze");
        Console.ReadKey();
    }
}
