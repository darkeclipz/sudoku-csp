using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CspSolver.Solver;

namespace CspSolver.Models
{
    public class ProjectEulerSuDoku
    {
        public static IEnumerable<string> PuzzleIterator()
        {
            var lines = File.ReadAllLines("puzzles.txt");
            for(int i = 1; i < lines.Length; i += 10)            
                yield return string.Join("", lines[i..(i+9)]);
        }

        public static void SolveProjectEulerPuzzle()
        {
            var threeDigitSum = 0;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var n = 1;
            foreach(var puzzle in PuzzleIterator())
            {
                var builder = SudokuModel.GetModel(puzzle);
                var model = builder.BuildCspModel();
                var solver = new BacktrackSearcher(model);
                var state = solver.Solve();
                int threeDigitNumber = model.Variables[0].Value * 100 
                                     + model.Variables[1].Value * 10 
                                     + model.Variables[2].Value;
                threeDigitSum += threeDigitNumber;

                Console.WriteLine($"Assignments for puzzle {n++}: {solver.Statistics.TotalAssigments}");
            }
            stopwatch.Stop();
            Console.WriteLine($"Solution: {threeDigitSum}");
            Console.WriteLine($"Total runtime: {stopwatch.Elapsed} sec.");
        }
    }
}