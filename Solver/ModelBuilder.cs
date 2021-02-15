using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    // CSP model which holds the variables, domains, and the constraints.
    public class ModelBuilder
    {
        public List<Variable> Variables = new List<Variable>();
        public List<int> Domain = new List<int>();
        public List<Constraint> Constraints = new List<Constraint>();

        // Create a new domain, which are the values that a variable
        // can be set to.
        public Domain CreateDomain(params int[] values)
        {
            var domain = new Domain();
            domain.Values.AddRange(values);

            if(domain.Values.Count != domain.Values.Distinct().Count())
            {
                throw new Exception("All values must be unique.");
            }

            return domain;
        }

        public BooleanDomain CreateBooleanDomain()
        {
            return new BooleanDomain();
        }

        // Create a new variable with a given domain.
        public Variable CreateVariable(string name, Domain domain)
        {
            var variable = new Variable
            {
                Id = Variables.Count,
                Name = name
            };
            Variables.Add(variable);
            variable.AddDomain(domain);
            return variable;
        }

        // Create constraint which requires all the variables to have different
        // values.
        public Constraint CreateAllDifferentConstraint(params Variable[] variables)
        {
            var constraint = new AllDifferentConstraint(variables);
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint CreateMandatoryConstraint(Variable parent, Variable child)
        {
            var constraint = new MandatoryConstraint(parent, child);
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint CreateOptionalConstraint(Variable parent, Variable child)
        {
            var constraint = new OptionalConstraint(parent, child);
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint CreateAlternativeConstraint(Variable parent, params Variable[] children)
        {
            var constraint = new AlternativeConstraint(parent, children);
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint CreateOrConstraint(Variable parent, params Variable[] children)
        {
            var constraint = new OrConstraint(parent, children);
            Constraints.Add(constraint);
            return constraint;
        }
        
        public Constraint CreateRequiredConstraint(Variable parent, Variable child)
        {
            var constraint = new RequiredConstraint(parent, child);
            Constraints.Add(constraint);
            return constraint;
        }

        public Constraint CreateExcludeConstraint(Variable parent, Variable child)
        {
            var constraint = new ExcludeConstraint(parent, child);
            Constraints.Add(constraint);
            return constraint;
        }

        public CspModel BuildCspModel()
        {
            var model = new CspModel
            {
                Variables = this.Variables.ToArray(),
                Constraints = this.Constraints.ToArray()
            };
            return model;
        }
    }
}