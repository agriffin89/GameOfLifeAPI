using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using GameOfLifeAPI.Models;
using GameOfLifeAPI.Services;
using GameOfLifeAPI.Data;
using Microsoft.Extensions.Logging;

namespace GameOfLifeAPI.Controllers
{
    [ApiController]
    [Route("api/boards")]
    public class GameOfLifeController : ControllerBase
    {
        private static Dictionary<Guid, Board> Boards = new Dictionary<Guid, Board>();
        private readonly GameOfLifeService _GameOfLifeService;
        private static readonly string StoragePath = "boards.json"; // Path for saving board states
        private readonly GameOfLifeContext _context;
        private readonly ILogger<GameOfLifeController> _logger;

        public GameOfLifeController(GameOfLifeContext context, ILogger<GameOfLifeController> logger, ILogger<GameOfLifeService> serviceLogger)
        {
            _GameOfLifeService = new GameOfLifeService(serviceLogger);
            LoadBoardsFromFile(); // Load board states on startup
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new board with the given state.
        /// </summary>
        /// <param name="board">Board object containing the initial state.</param>
        /// <returns>Returns the board ID upon success.</returns>
        /// <response code="200">Successfully created board.</response>
        /// <response code="400">Invalid board data provided.</response>
        [HttpPost("create")]
        public IActionResult CreateBoard([FromBody] Board board)
        {
            if (board == null || board.Grid == null || board.Grid.Count != board.Rows ||
                board.Grid[0].Count != board.Cols)
                return BadRequest("Invalid board data.");

            board.Id = Guid.NewGuid();
            Boards[board.Id] = board;
            SaveBoardsToFile();

            return Ok(new { boardId = board.Id });
        }

        /// <summary>
        /// Gets the next state of the board.
        /// </summary>
        /// <param name="id">Board ID.</param>
        /// <returns>Next state of the board.</returns>
        [HttpGet("{id}/next")]
        public IActionResult GetNextState(Guid id)
        {
            if (!Boards.ContainsKey(id))
                return NotFound("Board not found.");

            var board = Boards[id];

            //  Convert List<List<int>> to int[,] for processing
            int[,] grid = Board.ConvertTo2DArray(board.Grid);

            //  Process the next state (assuming GameOfLifeService handles int[,])
            int[,] nextState = _GameOfLifeService.GetNextState(grid, board.Rows, board.Cols);

            //  Convert back to List<List<int>> for JSON response
            board.Grid = Board.ConvertToList(nextState);

            return Ok(board);
        }

        /// <summary>
        /// Gets the state of the board after X generations.
        /// </summary>
        /// <param name="id">Board ID.</param>
        /// <param name="x">Number of generations to simulate.</param>
        /// <returns>Board state after X generations.</returns>
        [HttpGet("{id}/next/{x}")]
        public IActionResult GetNextXStates(Guid id, int x)
        {
            if (!Boards.ContainsKey(id))
                return NotFound("Board not found.");

            var board = Boards[id];

            //  Convert List<List<int>> to int[,] before processing
            int[,] grid = Board.ConvertTo2DArray(board.Grid);

            for (int i = 0; i < x; i++)
            {
                grid = _GameOfLifeService.GetNextState(grid, board.Rows, board.Cols);
            }

            //  Convert back to List<List<int>> after processing
            board.Grid = Board.ConvertToList(grid);

            SaveBoardsToFile();
            return Ok(board);
        }


        /// <summary>
        /// Gets the final stabilized state of the board.
        /// If the board does not stabilize within the max iterations, returns an error.
        /// </summary>
        /// <param name="id">Board ID.</param>
        /// <param name="maxIterations">Maximum number of iterations to check for stability.</param>
        /// <returns>Final state of the board or an error if unstable.</returns>
        [HttpGet("{id}/final")]
        public IActionResult GetFinalState(Guid id, [FromQuery] int maxIterations = 100)
        {
            try
            {
                if (!Boards.ContainsKey(id))
                {
                    _logger.LogWarning($"Board with ID {id} not found.");
                    return NotFound("Board not found.");
                }

                var board = Boards[id];

                //  Convert List<List<int>> to int[,] before calling service
                int[,] grid = Board.ConvertTo2DArray(board.Grid);

                var (finalGrid, stabilized) = _GameOfLifeService.GetFinalState(grid, board.Rows, board.Cols, maxIterations);

                if (!stabilized)
                {
                    _logger.LogInformation($"Board {id} did not stabilize after {maxIterations} iterations.");
                    return BadRequest(new { message = "Board did not stabilize within the given iterations." });
                }

                //  Convert int[,] back to List<List<int>> for storage
                board.Grid = Board.ConvertToList(finalGrid);
                SaveBoardsToFile();
                return Ok(board);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while processing board {id}.");
                return StatusCode(500, "Internal Server Error.");
            }
        }


        /// <summary>
        /// Saves board states to a file for persistence.
        /// </summary>
        private void SaveBoardsToFile()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Boards, Formatting.Indented);
                System.IO.File.WriteAllText(StoragePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving boards: {ex.Message}");
            }
        }

        /// <summary>
        /// Loads board states from the file if it exists.
        /// </summary>
        private void LoadBoardsFromFile()
        {
            try
            {
                if (System.IO.File.Exists(StoragePath))
                {
                    string json = System.IO.File.ReadAllText(StoragePath);
                    Boards = JsonConvert.DeserializeObject<Dictionary<Guid, Board>>(json) ?? new Dictionary<Guid, Board>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading boards: {ex.Message}");
                Boards = new Dictionary<Guid, Board>(); // Ensure initialization even if loading fails
            }
        }

        /// <summary>
        /// Saves the current board state.
        /// </summary>
        [HttpPost("save")]
        public IActionResult SaveBoard([FromBody] Board board)
        {
            board.SerializeState();  // Store as a string for persistence
            _context.Boards.Add(board);
            _context.SaveChanges();
            return Ok(board);
        }

        /// <summary>
        /// Loads a previously saved board state from storage.
        /// </summary>
        /// <param name="id">The unique identifier of the board.</param>
        /// <returns>
        /// Returns the stored board if found; otherwise, returns a 404 Not Found response.
        /// </returns>
        /// <response code="200">Successfully retrieved the board state.</response>
        /// <response code="404">Board with the given ID was not found.</response>
        [HttpGet("boards/load/{id}")]
        public IActionResult LoadBoard(Guid id)
        {
            LoadBoardsFromFile(); // Load saved boards from storage

            if (!Boards.ContainsKey(id)) // Check if board exists
                return NotFound("Board not found."); // Return 404 Not Found if board doesn't exist

            return Ok(Boards[id]); // Return the board data
        }

    }
}
