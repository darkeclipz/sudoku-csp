using System;
using System.Collections.Generic;

namespace CspSolver.Solver
{
    public abstract class Constraint
    {
        public Guid Id = Guid.NewGuid();
        public Guid[] VariableKeys;

        public abstract bool IsSatisfied(Model model);
        public bool VariablesSet(Model model)
        {
            for(int i = 0; i < VariableKeys.Length; i++)
            {
                if(!model.Variables[VariableKeys[i]].IsSet)
                {
                    return false;
                }
            }
            return true;
        }
    }

    public class AllDifferentConstraint : Constraint 
    {
        public AllDifferentConstraint(params Guid[] variableKeys)
        {
            VariableKeys = variableKeys;
        }

        public override bool IsSatisfied(Model model)
        {
            var values = new Dictionary<int, bool>();
            for(int i = 0; i < VariableKeys.Length; i++)
            {
                Guid guid = VariableKeys[i];
                Variable variable = model.Variables[guid];
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