using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    public class Domain
    {
        public List<int> Values = new List<int>();

        public static Domain Copy(Domain domain) 
        {
            var copy = (Domain)domain.MemberwiseClone();
            copy.Values = domain.Values.ToList();
            return copy;
        }
    }
}