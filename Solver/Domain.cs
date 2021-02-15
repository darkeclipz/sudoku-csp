using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    public class Domain
    {
        public List<int> Values = new List<int>();

        public virtual Domain Copy() 
        {
            var copy = (Domain)this.MemberwiseClone();
            copy.Values = this.Values.ToList();
            return copy;
        }
    }

    public class BooleanDomain : Domain
    {
        public BooleanDomain()
        {
            Values = new List<int> { 0, 1 };
        }

        public override Domain Copy()
        {
            var copy = (BooleanDomain)this.MemberwiseClone();
            copy.Values = this.Values.ToList();
            return copy;
        }
    }
}