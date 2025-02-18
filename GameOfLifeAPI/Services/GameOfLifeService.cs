using GameOfLifeAPI.Models;
using System.Collections.Generic;
using System.Text;

namespace GameOfLifeAPI.Services
{
    public class GameOfLifeService
    {
        private const int ALIVE = 1;
        private const int DEAD = 0;
        private readonly ILogger<GameOfLifeService> _logger;

        public GameOfLifeService(ILogger<GameOfLifeService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Computes the next state of the given board based on Conway's Game of Life rules.
        /// </summary>
        /// <param name="grid">Current state of the board.</param>
        /// <param name="rows">Number of rows in the board.</param>
        /// <param name="cols">Number of columns in the board.</param>
        /// <returns>New state of the board after applying the rules.</returns>
        public int[,] GetNextState(int[,] grid, int rows, int cols)
        {
            int[,] newGrid = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int liveNeighbors = CountLiveNeighbors(grid, i, j, rows, cols);
                    bool isAlive = grid[i, j] == 1;

                    if (isAlive && (liveNeighbors < 2 || liveNeighbors > 3))
                        newGrid[i, j] = 0; // Cell dies
                    else if (!isAlive && liveNeighbors == 3)
                        newGrid[i, j] = 1; // Cell comes to life
                    else
                        newGrid[i, j] = grid[i, j]; // No change
                }
            }

            return newGrid;
        }


        /// <summary>
        /// Counts the number of live neighbors for a given cell in the grid.
        /// </summary>
        /// <param name="grid">Current state of the board.</param>
        /// <param name="x">Row index of the cell.</param>
        /// <param name="y">Column index of the cell.</param>
        /// <param name="rows">Number of rows in the board.</param>
        /// <param name="cols">Number of columns in the board.</param>
        /// <returns>Number of live neighboring cells.</returns>
        private int CountLiveNeighbors(int[,] grid, int x, int y, int rows, int cols)
        {
            int[] dx = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] dy = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int count = 0;

            for (int i = 0; i < 8; i++)
            {
                int newX = x + dx[i];
                int newY = y + dy[i];

                if (newX >= 0 && newX < rows && newY >= 0 && newY < cols && grid[newX, newY] == ALIVE)
                    count++;
            }

            return count;
        }

        /// <summary>
        /// Simulates the board's evolution until it reaches a stable state or a maximum number of iterations.
        /// </summary>
        /// <param name="grid">Initial state of the board.</param>
        /// <param name="rows">Number of rows in the board.</param>
        /// <param name="cols">Number of columns in the board.</param>
        /// <param name="maxIterations">Maximum iterations before stopping.</param>
        /// <returns>Final stable state of the board and a boolean indicating if it stabilized.</returns>
        public (int[,], bool) GetFinalState(int[,] grid, int rows, int cols, int maxIterations)
        {
            try
            {
                HashSet<string> previousStates = new HashSet<string>();

                for (int i = 0; i < maxIterations; i++)
                {
                    string gridState = GridToString(grid);
                    if (previousStates.Contains(gridState))
                    {
                        _logger.LogInformation($"Board stabilized after {i + 1} iterations.");
                        return (grid, true);
                    }

                    previousStates.Add(gridState);
                    grid = GetNextState(grid, rows, cols);
                }

                _logger.LogWarning($"Board did not stabilize after {maxIterations} iterations.");
                return (grid, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error computing final state.");
                throw;
            }
        }

        //  NEW: Method to check if two grids are identical
        private bool AreGridsEqual(int[,] grid1, int[,] grid2)
        {
            for (int i = 0; i < grid1.GetLength(0); i++)
            {
                for (int j = 0; j < grid1.GetLength(1); j++)
                {
                    if (grid1[i, j] != grid2[i, j]) return false;
                }
            }
            return true;
        }

        // Helper method to check if the grid is completely dead
        private bool IsGridEmpty(int[,] grid)
        {
            foreach (int cell in grid)
            {
                if (cell == 1) return false; // Grid still has live cells
            }
            return true; // All cells are dead
        }

        /// <summary>
        /// Converts the board's state to a string representation for easy comparison.
        /// </summary>
        /// <param name="grid">Current state of the board.</param>
        /// <returns>String representation of the board state.</returns>
        private string GridToString(int[,] grid)
        {
            int rows = grid.GetLength(0);
            int cols = grid.GetLength(1);
            StringBuilder result = new StringBuilder();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result.Append(grid[i, j]);
                }
                result.Append("\n"); //  Ensures a consistent newline format
            }

            return result.ToString().Replace("\r\n", "\n"); //  Normalize newlines across OS
        }

    }
}
