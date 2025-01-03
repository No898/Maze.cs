using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
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

            // Validace: počet char v řádku pro kontrolu 2d
            int expectedLength = lines[0].Length;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length != expectedLength)
                {
                    Console.WriteLine($"Chyby na řádku {i + 1} má délku {lines[i].Length}, ale očekává se {expectedLength}.");
                    WaitForExit();
                    return;
                }
            }

            // Vytvoření pole pro 
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

           // Zobrazení 2d pole
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(maze[i, j]);
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Došlo k chybě při čtení souboru: {ex.Message}");
        }
        WaitForExit();

    }

    // Potvrzení o ukončení programu
    static void WaitForExit()
    {
        Console.WriteLine("\nStiskni libovolnou klávesu pro ukončení programu...");
        Console.ReadKey();
    }
}
