using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    public class Domain
    {
        public Guid Id = Guid.NewGuid();
        public string Name;
        public List<int> Values = new List<int>();

        public static Domain Copy(Domain domain) 
        {
            var copy = (Domain)domain.MemberwiseClone();
            copy.Id = domain.Id;
            copy.Values = domain.Values.ToList();
            return copy;
        }
    }
}