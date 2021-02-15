using System;
using System.Diagnostics;

namespace CspSolver.Solver
{
    // Holds different statistics for the recursive backtrack solver.
    public class BacktrackSearchStatistics
    {
        public int TotalAssigments;
        public Stopwatch Stopwatch = new Stopwatch();

        public void Print()
        {
            Console.WriteLine($"Assignments: {TotalAssigments}");
            Console.WriteLine($"Time elapsed: {Stopwatch.Elapsed} sec.");
        }

        public void PrintModel(CspModel model)
        {
            foreach(var variable in model.Variables)
            {
                Console.WriteLine($"{variable.Name} {{{string.Join(", ", variable.Domain.Values)}}}"
                    + $" = {variable.Value} (Set: {variable.IsSet})");
            }
        }
    }
}