using System;
using System.Linq;

/// <summary>
/// Helper class to provide extensions for 2D arrays.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Extracts a row from a 2D array.
    /// </summary>
    public static int[] GetRow(this int[,] array, int row)
    {
        return Enumerable.Range(0, array.GetLength(1))
                         .Select(col => array[row, col])
                         .ToArray();
    }
}
