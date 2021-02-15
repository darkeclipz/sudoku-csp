using System;
using System.Collections.Generic;

namespace CspSolver.Solver
{
    public abstract class Constraint
    {
        public Guid Id = Guid.NewGuid();
        public Variable[] Variables;

        public abstract bool IsSatisfied(CspModel model);
        public bool VariablesSet(CspModel model)
        {
            for(int i = 0; i < Variables.Length; i++)
            {
                var id = Variables[i].Id;
                if(!model.Variables[id].IsSet)
                {
                    return false;
                }
            }
            return true;
        }
        public bool ContainsVariable(int variable)
        {
            for(int i = 0; i < Variables.Length; i++)
            {
                if(Variables[i].Id == variable)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class AllDifferentConstraint : Constraint 
    {
        public AllDifferentConstraint(params Variable[] variables)
        {
            Variables = variables;
        }

        public override bool IsSatisfied(CspModel model)
        {
            var values = new Dictionary<int, bool>();
            for(int i = 0; i < Variables.Length; i++)
            {
                Variable variable = model.Variables[i];
                if(values.ContainsKey(variable.Value))
                {
                    return false;
                }
                values.Add(variable.Value, true);
            }
            return true;
        }
    }
}