using System;
using System.Collections.Generic;
using System.Text;
using CspSolver.Solver;

namespace CspSolver.Models
{
    public class SudokuModel
    {
        public static ModelBuilder GetModel(string sudokuString = "")
        {
            var model = new ModelBuilder();
            var domain = model.CreateDomain("values", 1, 2, 3, 4, 5, 6, 7, 8, 9);

            // Create all the variable (9*9 = 81)
            var variables = new Variable[9 * 9];
            for(int i = 0; i < 9 * 9; i++)
            {
                variables[i] = model.CreateVariable($"Cell {i}", domain);
            } 

            // Create all the AllDiff constraints for the rows.
            for(int i = 0; i < 9 * 9; i += 9)
            {
                var row = new Variable[9];
                for(int j = 0; j < 9; j++)
                {
                    row[j] = variables[i+j];
                }
                model.CreateAllDifferentConstraint(row);
            }

            // Create all the AllDiff constraints for the cols.
            for(int i = 0; i < 9; i++)
            {
                var col = new Variable[9];
                for(int j = 0; j < 9 * 9; j += 9)
                {
                    col[j/9] = variables[i + j];
                }
                model.CreateAllDifferentConstraint(col);
            }

            // Create all the AllDiff constraints for the squares.
            var squares = new List<int[]>
            {
                new int[9] { 0, 1, 2, 9, 10, 11, 18, 19, 20 },
                new int[9] { 3, 4, 5, 12, 13, 14, 21, 22, 23 },
                new int[9] { 6, 7, 8, 15, 16, 17, 24, 25, 26 },
                new int[9] { 27, 28, 29, 36, 37, 38, 45, 46, 47 },
                new int[9] { 30, 31, 32, 39, 40, 41, 48, 49, 50 },
                new int[9] { 33, 34, 35, 42, 43, 44, 51, 52, 53 },
                new int[9] { 54, 55, 56, 63, 64, 65, 72, 73, 74 },
                new int[9] { 57, 58, 59, 66, 67, 68, 75, 76, 77 },
                new int[9] { 60, 61, 62, 69, 70, 71, 78, 79, 80 }
            };
            for(int i = 0; i < 9; i++)
            {
                var square = new Variable[9];
                for(int j = 0; j < 9; j++)
                {
                    square[j] = variables[squares[i][j]];
                }
                model.CreateAllDifferentConstraint(square);
            }

            // Set the assigned cells from the sudoku string.
            if(sudokuString.Length == 9*9)
            {
                for(int i = 0; i < 9 * 9; i++)
                {
                    char c = sudokuString[i];
                    if(c >= '1' && c <= '9')
                    {
                        variables[i].Value = int.Parse(c.ToString());
                        variables[i].IsSet = true;    
                    }
                }
            }
            return model;
        }

        public static void PrintBoard(CspModel model)
        {
            var sb = new StringBuilder();
            for(int i = 0; i < 9; i++)
            {
                for(int j = 0; j < 9; j++)
                {   
                    char c = model.Variables[9*i+j].Value.ToString()[0];
                    sb.Append(c == '0' ? '.' : c);
                }

                sb.AppendLine();
            }
            Console.WriteLine(sb.ToString());
        }
    }
}