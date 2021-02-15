using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CspSolver.Solver
{
    public class ConstraintPropagator
    {
        private CspModel Model;

        public ConstraintPropagator(CspModel model)
        {
            Model = model;
        }

        // Forward propagation of the assigned value. The value will be removed
        // from all the domains of the other variables which are shared by the same constraint.
        public Propagation PropagateAssignment(Variable variable, int value)
        {
            var propagation = new Propagation();
            propagation.IsValid = true;

            // Check each constraint that contains the assigned variable.
            for(int i = 0; i < Model.Constraints.Length; i++)
            {
                var constraint = Model.Constraints[i];
                if(constraint.ContainsVariable(variable.Id))
                {
                    switch(constraint)
                    {
                        case AllDifferentConstraint:
                            PropagateAllDifferentConstraint(variable, value, constraint, ref propagation);
                            break;
                        default:
                            Debug.WriteLine($"Constraint of type '{constraint.GetType().Name}' does not support constraint propagation.");
                            break;
                    }

                    if (!propagation.IsValid)
                    {
                        return propagation;
                    }
                }
            }
            return propagation;
        }

        private void PropagateAllDifferentConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // Check all the other variables that are shared in this constraint.
            for(int i = 0; i < constraint.Variables.Length; i++)
            {
                var otherVariable = constraint.Variables[i];

                // If the variable is set, and the domain of the other variable contains the
                // assigned value.
                if(!otherVariable.IsSet && otherVariable.Domain.Values.Contains(value))
                {
                    // Remove the value from the domain of the variable which shares a
                    // constraint with the assigned variable.
                    propagation.Add(otherVariable.Id, value);
                    otherVariable.Domain.Values.Remove(value);

                    // If the domain is reduced to zero, the propagation resulted in 
                    // an inconsistent model. Set the flag, and return the reduced domains
                    // so they can be restored.
                    if(otherVariable.Domain.Values.Count == 0)
                    {
                        propagation.IsValid = false;
                        return;
                    }
                }
            }
        }

        // Restore the domains of the variables which where reduced.
        public void UndoPropagation(Propagation propagation)
        {
            foreach(var reduction in propagation.Reductions)
            {
                Model.Variables[reduction.Variable].Domain.Values.Add(reduction.Value);
            }
        }
    }
}