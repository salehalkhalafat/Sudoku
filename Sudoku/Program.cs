using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;

namespace Sudoku;


// Main game logic and helper functions for generating and displaying the board.
class Game
{
    // Contains global configuration values and the game boards.
    // - `boardSize`: size of the board (9 for a standard Sudoku)
    // - `subGridSize`: width/height of a subgrid (3 for a standard Sudoku)
    // - `emptyCells`: number of cells to remove for the puzzle (not currently used)
    // - `Board`: the playable board the user interacts with
    // - `Solution`: a copy of the generated solution (filled board)
    static int boardSize = 9;
    static int subGridSize = 3;
    static int emptyCells = 50;
    static int AnswerRow;
    static int AnswerCol;
    static int AnswerNum;
    static int[,] Board = new int[boardSize, boardSize];
    static int[,] Solution = new int[boardSize, boardSize];

    // Random number generator used across helper methods.
    static Random rand = new Random();

    // Program entry point. Initializes the game state and enters the input loop.
    // - args: command-line arguments (not used)
    static void Main(string[] args)
    {
        Console.WriteLine("Welcome to Sudoku!");
        BoardSetup(Board, Solution, emptyCells);
        while (true)
        {
            PlayerInput(Board, Solution);
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
        int size = boardSize;
        int box = subGridSize;
        for (int i = 0; i < size; i++)
        {
            if (i == 0)
            {
                Console.Write("  ");
            }
            if (i % box == 0)
            {
                Column();
            }
            Console.Write($"{i + 1} ");
        }
        Console.WriteLine();
        Row();
        Console.ResetColor();
    }

    // Prints the vertical separator used between 3x3 subgrids.
    static void Column()
    {
        GreenColor();
        Console.Write("| ");
    }
    // Prints the horizontal separator used between 3x3 subgrid rows.
    static void Row()
    {
        GreenColor();
        Console.WriteLine("--------------------------");
    }

    // Validates whether `number` can be placed at position (row, column)
    // without violating Sudoku constraints (row, column, subgrid).
    private static bool IsValid(int[,] board, int row, int column, int number)
    {

        // Check row
        for (int c = 0; c < boardSize; c++)
            if (board[row, c] == number)
                return false;

        // Check column
        for (int r = 0; r < boardSize; r++)
            if (board[r, column] == number)
                return false;

        // Check subgrid
        int startRow = (row / subGridSize) * subGridSize;
        int startCol = (column / subGridSize) * subGridSize;
        for (int r = startRow; r < startRow + subGridSize; r++)
            for (int c = startCol; c < startCol + subGridSize; c++)
                if (board[r, c] == number)
                    return false;

        return true;
    }

    // Backtracking fill for the entire board. Visits cells in row-major order
    // and tries numbers 1..boardSize in random order.
    static bool FillBoard(int[,] board, int row = 0, int column = 0)
    {

        if (row == boardSize)
            return true; // finished

        int nextRow = (column == boardSize - 1) ? row + 1 : row;
        int nextCol = (column == boardSize - 1) ? 0 : column + 1;

        if (board[row, column] != 0)
            return FillBoard(board, nextRow, nextCol);

        var nums = Enumerable.Range(1, boardSize).OrderBy(_ => rand.Next()).ToArray();
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

        for (int row = 0; row < boardSize; row++)
        {
            if (row % subGridSize == 0 && row != 0)
            {
                Row();
            }
            for (int column = 0; column < boardSize; column++)
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
                if (column % subGridSize == 0 && column != 0)
                {
                    Column();
                    Console.ResetColor();
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
        AnswerRow = int.Parse(Console.ReadLine());
        InputError(AnswerRow);
        Console.WriteLine("Enter the column (1-9): ");
        AnswerCol = int.Parse(Console.ReadLine());
        InputError(AnswerCol);
        if (board[AnswerRow - 1, AnswerCol - 1] != 0)
        {
            RedColor();
            Console.WriteLine("Cell already filled. Try again.");
            Console.ResetColor();
            PlayerInput(board, solution);
            return;
        }
        Console.WriteLine("Enter the number (1-9): ");
        AnswerNum = int.Parse(Console.ReadLine());
        InputError(AnswerNum);

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
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
                destination[r, c] = source[r, c];
    }
    // Fills the entire board with a valid Sudoku solution using backtracking
    // and prints the filled board.
    static void FillGrid(int[,] board)
    {
        // clear board
        for (int r = 0; r < boardSize; r++)
            for (int c = 0; c < boardSize; c++)
                board[r, c] = 0;

        if (!FillBoard(board))
        {
            Console.WriteLine("Failed to generate a complete board.");
            return;
        }
    }

    static void EmptyCells(int[,] board, int cellsToRemove)
    {
        int removed = 0;
        while (removed < cellsToRemove)
        {
            int r = rand.Next(boardSize);
            int c = rand.Next(boardSize);
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
    static bool InputError(int userinput)
    {
        if (userinput >= 1 && userinput <= 9)
        {
            return true;
        }
        else
        {
            RedColor();
            Console.WriteLine("Invalid input. Please enter numbers between 1 and 9.");
            Console.ResetColor();
            PlayerInput(Board, Solution);
            return false;
        }
    }
}
