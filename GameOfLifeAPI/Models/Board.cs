using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;

namespace GameOfLifeAPI.Models
{
    public class Board
    {
        /// <summary>
        /// Unique identifier for the board.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Number of rows in the board.
        /// </summary>
        [Required]
        public int Rows { get; set; }

        /// <summary>
        /// Number of columns in the board.
        /// </summary>
        [Required]
        public int Cols { get; set; }

        /// <summary>
        /// 2D grid representation of the board.
        /// </summary>
        [Required]
        public List<List<int>> Grid { get; set; } = new List<List<int>>()
        {
            new List<int> { 0, 1, 0 },
            new List<int> { 1, 1, 1 },
            new List<int> { 0, 1, 0 }
        };

        /// <summary>
        /// String representation of the board.
        /// </summary>
        [Required]
        public string State { get; set; } = "010\n111\n010\n";

        public Board()
        {
            //  Initialize Grid with empty rows and columns
            for (int i = 0; i < Rows; i++)
            {
                Grid.Add(new List<int>(new int[Cols]));
            }
        }

        //  Convert List<List<int>> to int[,]
        public static int[,] ConvertTo2DArray(List<List<int>> list)
        {
            if (list == null || list.Count == 0) return new int[0, 0];

            int rows = list.Count;
            int cols = list[0].Count;
            int[,] array = new int[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    array[i, j] = list[i][j];
                }
            }
            return array;
        }

        //  Convert int[,] to List<List<int>>
        public static List<List<int>> ConvertToList(int[,] array)
        {
            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            List<List<int>> list = new List<List<int>>();

            for (int i = 0; i < rows; i++)
            {
                List<int> row = new List<int>();
                for (int j = 0; j < cols; j++)
                {
                    row.Add(array[i, j]);
                }
                list.Add(row);
            }
            return list;
        }

        //  Convert Grid to a string format for storage
        public void SerializeState()
        {
            State = string.Join("\n", Grid.Select(row => string.Join("", row)));
        }

        //  Convert State string back to Grid format
        public void DeserializeState()
        {
            if (string.IsNullOrEmpty(State)) return;
            var rows = State.Split('\n');
            Grid = rows.Select(row => row.Select(c => c - '0').ToList()).ToList();
        }
    }
}
