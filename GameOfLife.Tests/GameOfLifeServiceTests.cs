using Xunit;
using GameOfLifeAPI.Services;
using GameOfLifeAPI.Models;
using Microsoft.Extensions.Logging;
using Moq; 
public class GameOfLifeServiceTests
{
    private readonly GameOfLifeService _service;

    public GameOfLifeServiceTests()
    {
        //  Create a mock logger
        var mockLogger = new Mock<ILogger<GameOfLifeService>>();

        //  Pass the mock logger into the service
        _service = new GameOfLifeService(mockLogger.Object);
    }
    /// <summary>
    /// Tests the GetNextState method to ensure the grid updates correctly
    /// according to Conway's Game of Life rules.
    /// </summary>
    [Fact]
    public void GetNextState_ChangesGridState()
    {
        // Arrange: Define an initial 3x3 grid with a known state.
        // This grid represents a "Blinker" pattern, which should oscillate.
        int[,] initialGrid = {
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 }
    };

        // Expected next state after one iteration:
        // The vertical line should become horizontal.
        int[,] expectedNextState = {
        { 0, 0, 0 },
        { 1, 1, 1 },
        { 0, 0, 0 }
    };

        // Act: Call the GetNextState method to compute the next state.
        int[,] nextState = _service.GetNextState(initialGrid, 3, 3);

        // Assert: Check if the computed next state matches the expected state.
        Assert.Equal(expectedNextState, nextState);
    }

    /// <summary>
    /// Tests if the GetNextState method correctly counts live neighbors.
    /// Since CountLiveNeighbors is private, we validate its behavior through GetNextState.
    /// </summary>
    [Fact]
    public void CountLiveNeighbors_IsCorrectThroughGetNextState()
    {
        // Arrange: Define a 3x3 grid with a known pattern.
        int[,] initialGrid = {
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 }
    };

        // Expected next state:
        // The middle cell (1,1) should have 4 neighbors and die.
        int[,] expectedNextState = {
        { 1, 1, 1 },
        { 1, 0, 1 },
        { 1, 1, 1 }
    };

        // Act: Get the next state (which internally calls CountLiveNeighbors).
        int[,] nextState = _service.GetNextState(initialGrid, 3, 3);

        // Assert: Verify if the grid evolved as expected.
        Assert.Equal(expectedNextState, nextState);
    }


    /// <summary>
    /// Test GetFinalState method for a board that stabilizes.
    /// </summary>
    [Fact]
    public void GetFinalState_StabilizesCorrectly()
    {
        // Arrange: Define an initial 3x3 grid (Blinker)
        int[,] initialGrid = {
        { 0, 1, 0 },
        { 1, 1, 1 },
        { 0, 1, 0 }
    };

        // Act: Run GetFinalState with more iterations
        (int[,] finalGrid, bool stabilized) = _service.GetFinalState(initialGrid, 3, 3, 50);

        // Print each row of the final stabilized state
        Console.WriteLine("Final Stabilized State:");
        for (int i = 0; i < finalGrid.GetLength(0); i++)
        {
            Console.WriteLine(string.Join(" ", finalGrid.GetRow(i)));
        }

        // Ensure the board actually stabilized
        Assert.True(stabilized, "The board did not stabilize correctly.");

        // Update the expected stable state if necessary
        int[,] expectedStableGrid = {
        { 0, 1, 0 },
        { 0, 1, 0 },
        { 0, 1, 0 }
    };

        // Assert: Compare expected and actual stabilized states
        Assert.Equal(expectedStableGrid, finalGrid);
    }


    /// <summary>
    /// Test GetFinalState when the board does NOT stabilize.
    /// </summary>
    [Fact]
    public void GetFinalState_DoesNotStabilize()
    {
        int[,] initialGrid = {
                { 1, 1, 1, 1 },
                { 1, 1, 1, 1 },
                { 1, 1, 1, 1 },
                { 1, 1, 1, 1 }
            };

        (int[,] finalGrid, bool stabilized) = _service.GetFinalState(initialGrid, 4, 4, 2);

        // The board is too large and may not stabilize within 2 iterations
        Assert.False(stabilized);
    }

    /// <summary>
    /// Tests if GridToString correctly converts a grid into a string.
    /// The test checks the format and expected output.
    /// </summary>
    [Fact]
    public void GridToString_ReturnsExpectedFormat()
    {
        // Arrange: Define a 3x3 grid.
        int[,] grid = {
        { 0, 1, 0 },
        { 1, 0, 1 },
        { 0, 1, 0 }
    };

        //  Ensure trailing newline in expected output
        string expectedString =
            "010\n" +
            "101\n" +
            "010\n";

        // Act: Use reflection to call the private GridToString method.
        var methodInfo = typeof(GameOfLifeService).GetMethod(
            "GridToString",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
        );

        Assert.NotNull(methodInfo); // Ensure method exists

        string result = (string)methodInfo.Invoke(_service, new object[] { grid })!;
        result = result.Replace("\r\n", "\n"); //  Normalize output to prevent OS newline issues

        // Assert: Compare expected and actual output
        Assert.Equal(expectedString, result);
    }

    [Fact]
    public void GetNextState_HandlesEmptyGrid()
    {
        // Arrange: Empty grid
        int[,] emptyGrid = new int[3, 3];

        // Act
        int[,] nextState = _service.GetNextState(emptyGrid, 3, 3);

        // Assert: The grid should remain unchanged
        Assert.Equal(emptyGrid, nextState);
    }

    [Fact]
    public void GetNextState_HandlesLargeGridPerformance()
    {
        // Arrange: Large 100x100 grid with random values
        int[,] largeGrid = new int[100, 100];
        Random random = new Random();
        for (int i = 0; i < 100; i++)
            for (int j = 0; j < 100; j++)
                largeGrid[i, j] = random.Next(2);

        // Act
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int[,] nextState = _service.GetNextState(largeGrid, 100, 100);
        watch.Stop();

        // Assert: The function should complete in under 500ms
        Assert.True(watch.ElapsedMilliseconds < 500, "GetNextState took too long.");
    }
}
