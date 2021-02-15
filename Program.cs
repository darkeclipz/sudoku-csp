using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CspSolver.Solver;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace CspSolver
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // var model = Models.Sudoku4x4.GetModel("341..2....2..143");
            var model = Models.Sudoku4x4.GetModel("..1..2....2..1..");
            //var model = Models.SudokuModel.GetModel(".3.9..7......5.3.91...78.24.4.5.2.......4....57.8132.....7.18.2......4.7.....5.1.");
            Console.WriteLine("-- Unsolved --");
            PrintModel(model);

            var solver = new BacktrackSolver(model);

            Console.WriteLine("-- Pruned --");
            solver.Propagate();
            PrintModel(model);

            Console.WriteLine("--- Solved for consistency ---");      
            solver.MakeArcConsistent();
            PrintModel(model);

            Console.WriteLine("-- Solver --");
            var isSolved = solver.Solve();

            Console.WriteLine("-- Solved --");
            PrintModel(model);

            Console.WriteLine($"Is Solved: {isSolved}");
            Console.ReadKey();
        }

        public static void PrintModel(Model model)
        {
            Console.WriteLine("Model:");
            foreach(var variable in model.Variables)
            {
                Console.WriteLine($"{variable.Value.Name} {{{string.Join(", ", variable.Value.Domain.Values)}}}"
                    + $" = {variable.Value.Value} (Set: {variable.Value.IsSet})");
            }
        }
    }
}
