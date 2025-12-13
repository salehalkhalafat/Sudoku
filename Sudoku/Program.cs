using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;


namespace Sudoku;

class Vars
{
    // Contains global configuration values and the game boards.
    // - `boardSize`: size of the board (9 for a standard Sudoku)
    // - `subGridSize`: width/height of a subgrid (3 for a standard Sudoku)
    // - `emptyCells`: number of cells to remove for the puzzle (not currently used)
    // - `Board`: the playable board the user interacts with
    // - `Solution`: a copy of the generated solution (filled board)
    public static int boardSize = 9;
    public static int subGridSize = 3;
    public static int emptyCells = 50;
    public int[,] Board = new int[boardSize, boardSize];
    public int[,] Solution = new int[boardSize, boardSize];
}

// Main game logic and helper functions for generating and displaying the board.
class Game
{
    // Random number generator used across helper methods.
    static Random rand = new Random();

    // Program entry point. Initializes the game state and enters the input loop.
    // - args: command-line arguments (not used)
    static void Main(string[] args)
    {
        Vars vars = new Vars();
        Console.WriteLine("Welcome to Sudoku!");
        BoardSetup(vars.Board, vars.Solution, Vars.emptyCells);
        while (true)
        {
            PlayerInput(vars.Board, vars.Solution);
        }
    }

    // Sets the console foreground color to red for highlighting errors.
    static void RedColor()
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
    }
    // Sets the console foreground color to blue (currently unused).
    static void BlueColor()
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Blue;
    }
    // Sets the console foreground color to green for labels and board grid.
    static void GreenColor()
    {
        ConsoleColor originalColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
    }
    // Prints the header row showing column indices and the top grid separator.
    static void HeaderRow()
    {
        GreenColor();
        Console.WriteLine("  | 1 2 3 | 4 5 6 | 7 8 9 ");
        Console.WriteLine("--------------------------");
        Console.ResetColor();
    }

    // Prints the vertical separator used between 3x3 subgrids.
    static void Column()
    {
        GreenColor();
        Console.Write("| ");
        Console.ResetColor();
    }
    // Prints the horizontal separator used between 3x3 subgrid rows.
    static void Row()
    {
        GreenColor();
        Console.WriteLine("--------------------------");
        Console.ResetColor();
    }

    // Validates whether `number` can be placed at position (row, column)
    // without violating Sudoku constraints (row, column, subgrid).
    private static bool IsValid(int[,] board, int row, int column, int number)
    {
        int size = Vars.boardSize;
        int box = Vars.subGridSize;

        // Check row
        for (int c = 0; c < size; c++)
            if (board[row, c] == number)
                return false;

        // Check column
        for (int r = 0; r < size; r++)
            if (board[r, column] == number)
                return false;

        // Check subgrid
        int startRow = (row / box) * box;
        int startCol = (column / box) * box;
        for (int r = startRow; r < startRow + box; r++)
            for (int c = startCol; c < startCol + box; c++)
                if (board[r, c] == number)
                    return false;

        return true;
    }

    // Backtracking fill for the entire board. Visits cells in row-major order
    // and tries numbers 1..boardSize in random order.
    static bool FillBoard(int[,] board, int row = 0, int column = 0)
    {
        int size = Vars.boardSize;

        if (row == size)
            return true; // finished

        int nextRow = (column == size - 1) ? row + 1 : row;
        int nextCol = (column == size - 1) ? 0 : column + 1;

        if (board[row, column] != 0)
            return FillBoard(board, nextRow, nextCol);

        var nums = Enumerable.Range(1, size).OrderBy(_ => rand.Next()).ToArray();
        foreach (var n in nums)
        {
            if (IsValid(board, row, column, n))
            {
                board[row, column] = n;
                if (FillBoard(board, nextRow, nextCol))
                    return true;
                board[row, column] = 0;
            }
        }

        return false;
    }


    // Prints the board to the console using a human-friendly grid layout.
    // Empty cells (0) are shown as blank spaces.
    static void PrintBoard(int[,] board)
    {
        for (int row = 0; row < 9; row++)
        {
            if (row % 3 == 0 && row != 0)
            {
                Row();
            }
            for (int column = 0; column < 9; column++)
            {
                if (row == 0 && column == 0)
                {
                    HeaderRow();
                }
                if (column == 0)
                {
                    GreenColor();
                    Console.Write(row + 1 + " | ");
                    Console.ResetColor();
                }
                if (column % 3 == 0 && column != 0)
                {
                    Column();
                }
                if (board[row, column] == 0)
                {
                    Console.Write("  ");
                }
                else
                {
                    Console.Write(board[row, column] + " ");
                }
            }
            Console.WriteLine();
        }
    }

    // Handles user input for placing a number on the board.
    // - Prompts for row, column and number (1-9), validates the move and updates the board.
    static void PlayerInput(int[,] board, int[,] solution)
    {
        Console.WriteLine("Enter the row (1-9): ");
        int AnswerRow = Convert.ToInt32(Console.ReadLine());
        Console.WriteLine("Enter the column (1-9): ");
        int AnswerCol = Convert.ToInt32(Console.ReadLine());
        if (board[AnswerRow - 1, AnswerCol - 1] != 0)
        {
            RedColor();
            Console.WriteLine("Cell already filled. Try again.");
            Console.ResetColor();
            PlayerInput(board, solution);
            return;
        }
        Console.WriteLine("Enter the number (1-9): ");
        int AnswerNum = Convert.ToInt32(Console.ReadLine());

        if (AnswerNum == solution[AnswerRow - 1, AnswerCol - 1])
        {
            board[AnswerRow - 1, AnswerCol - 1] = AnswerNum;
            PrintBoard(board);
        }
        else
        {
            Console.Clear();
            PrintBoard(board);
            RedColor();
            Console.WriteLine("Invalid move. Try again.");
            Console.ResetColor();
            PlayerInput(board, solution);
        }
    }

    static void CopyBoard(int[,] source, int[,] destination)
    {
        for (int r = 0; r < Vars.boardSize; r++)
            for (int c = 0; c < Vars.boardSize; c++)
                destination[r, c] = source[r, c];
    }
    // Fills the entire board with a valid Sudoku solution using backtracking
    // and prints the filled board.
    static void FillGrid(int[,] board)
    {
        // clear board
        for (int r = 0; r < Vars.boardSize; r++)
            for (int c = 0; c < Vars.boardSize; c++)
                board[r, c] = 0;

        if (!FillBoard(board))
        {
            Console.WriteLine("Failed to generate a complete board.");
            return;
        }
    }

    static void EmptyCells(int[,] board, int cellsToRemove)
    {
        int size = Vars.boardSize;
        int removed = 0;
        while (removed < cellsToRemove)
        {
            int r = rand.Next(size);
            int c = rand.Next(size);
            if (board[r, c] != 0)
            {
                board[r, c] = 0;
                removed++;
            }
        }
    }

    static void BoardSetup(int[,] board, int[,] solution, int emptycells)
    {
        FillGrid(solution);
        CopyBoard(solution, board);
        EmptyCells(board, emptycells);
        Console.Clear();
        PrintBoard(board);
    }
}
