using System.Collections.Generic;

namespace CspSolver.Solver
{
    public class Propagation
    {
        public bool IsValid;
        public List<Reduction> Reductions = new List<Reduction>();
        public void Add(int variable, int value)
        {
            Reductions.Add(new Reduction
            {
                Variable = variable,
                Value = value
            });
        }
    }
}