using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    // The model holds all the variables and constraints.
    public class CspModel
    {
        public Variable[] Variables;
        public Constraint[] Constraints;

        // Returns true if all the assigned variables satisfy the constraints.
        public bool IsConsistent()
        {
            for(int i = 0; i < Constraints.Length; i++)
            {
                if(!Constraints[i].IsSatisfied(this))
                {
                    return false;
                }
            }
            return true;
        }
    }
}