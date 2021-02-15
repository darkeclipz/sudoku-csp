using System.Collections.Generic;
using CspSolver.Solver;

namespace CspSolver.Models
{
    public class SudokuModel
    {
        public static Model GetModel(string sudokuString = "")
        {
            var model = new Model();
            var domain = model.CreateDomain("values", 1, 2, 3, 4, 5, 6, 7, 8, 9);
            var variables = new Variable[9 * 9];
            for(int i = 0; i < 9 * 9; i++)
            {
                variables[i] = model.CreateVariable($"Cell {i}", domain);
            }       
            for(int i = 0; i < 9 * 9; i += 9)
            {
                var row = new Variable[9];
                for(int j = 0; j < 9; j++)
                {
                    row[j] = variables[i+j];
                }
                model.CreateAllDifferentConstraint(row);
            }
            for(int i = 0; i < 9; i++)
            {
                var col = new Variable[9];
                for(int j = 0; j < 9 * 9; j += 9)
                {
                    col[j/9] = variables[i + j];
                }
                model.CreateAllDifferentConstraint(col);
            }
            if(sudokuString.Length == 9*9)
            {
                for(int i = 0; i < 9 * 9; i++)
                {
                    char c = sudokuString[i];
                    if(c < '1' || c > '9')
                    {
                        continue;
                    }
                    variables[i].Value = int.Parse(c.ToString());
                    variables[i].IsSet = true;
                }
            }
            return model;
        }
    }
}