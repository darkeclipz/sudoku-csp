using CspSolver.Solver;

namespace CspSolver.Models
{
    public class Sudoku4x4
    {
        public static Model GetModel(string puzzleString)
        {
            var model = new Model();
            var domain = model.CreateDomain("values", 1, 2, 3, 4);

            var variables = new Variable[16];
            for(int i = 0; i < 16; i++)
            {
                variables[i] = model.CreateVariable($"Cell {i + 1}", domain);
                char c = puzzleString[i];
                if(c >= '1' && c <= '9')
                {
                    variables[i].Value = int.Parse(c.ToString());
                    variables[i].IsSet = true;
                }
            }

            /*
            | 0  | 1  | 2  | 3 |
            | 4  | 5  | 6  | 7 | 
            | 8  | 9  | 10 | 11 |
            | 12 | 13 | 14 | 15 |
            */

            // Rows
            model.CreateAllDifferentConstraint(variables[0], variables[1], variables[2], variables[3]);
            model.CreateAllDifferentConstraint(variables[4], variables[5], variables[6], variables[7]);
            model.CreateAllDifferentConstraint(variables[8], variables[9], variables[10], variables[11]);
            model.CreateAllDifferentConstraint(variables[12], variables[13], variables[14], variables[15]);

            // Columns
            model.CreateAllDifferentConstraint(variables[0], variables[4], variables[8], variables[12]);
            model.CreateAllDifferentConstraint(variables[1], variables[5], variables[9], variables[13]);
            model.CreateAllDifferentConstraint(variables[2], variables[6], variables[10], variables[14]);
            model.CreateAllDifferentConstraint(variables[3], variables[7], variables[11], variables[15]);

            // Squares
            model.CreateAllDifferentConstraint(variables[0], variables[1], variables[4], variables[5]);
            model.CreateAllDifferentConstraint(variables[2], variables[3], variables[6], variables[7]);
            model.CreateAllDifferentConstraint(variables[8], variables[9], variables[12], variables[13]);
            model.CreateAllDifferentConstraint(variables[10], variables[11], variables[14], variables[15]);

            return model;
        }
    }
}