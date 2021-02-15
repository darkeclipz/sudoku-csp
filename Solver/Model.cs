using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    // CSP model which holds the variables, domains, and the constraints.
    public class Model
    {
        public Dictionary<Guid, Variable> Variables = new Dictionary<Guid, Variable>();
        public Dictionary<Guid, Constraint> Constraints = new Dictionary<Guid, Constraint>();

        // Create a new domain, which are the values that a variable
        // can be set to.
        public Domain CreateDomain(string name, params int[] values)
        {
            var domain = new Domain
            {
                Name = name
            };
            domain.Values.AddRange(values);

            if(domain.Values.Count != domain.Values.Distinct().Count())
            {
                throw new Exception("All values must be unique.");
            }

            return domain;
        }

        // Create a new variable with a given domain.
        public Variable CreateVariable(string name, Domain domain)
        {
            var variable = new Variable
            {
                Id = Guid.NewGuid(),
                Name = name
            };
            Variables.Add(variable.Id, variable);
            variable.AddDomain(domain);
            return variable;
        }

        // Create constraint which requires all the variables to have different
        // values.
        public Constraint CreateAllDifferentConstraint(params Variable[] variables)
        {
            var constraint = new AllDifferentConstraint(variables.Select(v => v.Id).ToArray());
            Constraints.Add(constraint.Id, constraint);
            return constraint;
        }

        // Returns true if all the constraints are satisfied (valid). If allowPartial is set
        // to true, then constraints will only be evaluated if all the variables are set, otherwise
        // the constraint passes as valid.
        public bool IsSatisfied(bool allowPartialAssignment)
        {
            Guid[] constraintKeys = Constraints.Keys.ToArray();

            for(int i = 0; i < constraintKeys.Length; i++)
            {
                Guid key = constraintKeys[i];
                bool variablesAreSet = Constraints[key].VariablesSet(this);

                if(allowPartialAssignment && !variablesAreSet)
                {
                    continue;
                }

                if(!Constraints[key].IsSatisfied(this))
                {
                    return false;
                }
            }

            return true;
        }
    }
}