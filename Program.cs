using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CspSolver.Models;
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
            
            // var builder = Models.Sudoku4x4.GetModel("341..2....2..143");
            // var builder = Models.Sudoku4x4.GetModel("..1..2....2..1..");
            // var builder = Models.SudokuModel.GetModel(".8.4..6............4.6...1.6.35...41......7....8....35.6..8..7....3.54.6.2..1..8.");
            // var builder = Models.SudokuModel.GetModel("800000000003600000070090200050007000000045700000100030001000068008500010090000400");
            // var builder = Models.SudokuModel.GetModel("");
            // var builder = Models.AustraliaModel.GetModel();

            // solve the Project Euler puzzle where we have to solve 50 puzzles.
            // ProjectEulerSuDoku.SolveProjectEulerPuzzle();
            // Console.ReadKey();
            // return;


            var builder = Models.PhoneFeatureModel.GetModel();

            var model = builder.BuildCspModel();

            Console.WriteLine("--- Model ---");
            //SudokuModel.PrintBoard(model);
            PrintModel(model);

            var solver = new BacktrackSearcher(model);

            Console.WriteLine("--- Solution ---");
            var state = solver.Solve();
            //SudokuModel.PrintBoard(model);
            PrintModel(model);
            Console.WriteLine($"State: {Enum.GetName(typeof(SearchState), state)}");
            solver.Statistics.Print();
            Console.ReadKey();
        }

        public static void PrintModel(CspModel model)
        {
            foreach(var variable in model.Variables)
            {
                Console.WriteLine($"{variable.Name} {{{string.Join(", ", variable.Domain.Values)}}}"
                    + $" = {variable.Value} (Set: {variable.IsSet})");
            }
        }
    }
}
