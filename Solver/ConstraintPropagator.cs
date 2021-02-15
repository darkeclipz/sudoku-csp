using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CspSolver.Solver
{
    public class ConstraintPropagator
    {
        private CspModel Model;

        private const int PARENT = 0;
        private const int CHILD = 1;
        private const int ON = 1;
        private const int OFF = 0;

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
                        case MandatoryConstraint:
                            PropagateMandatoryConstraint(variable, value, constraint, ref propagation);
                            break;
                        case OptionalConstraint:
                            PropagateOptionalConstraint(variable, value, constraint, ref propagation);
                            break;
                        case AlternativeConstraint:
                            PropagateAlternativeConstraint(variable, value, constraint, ref propagation);
                            break;
                        case OrConstraint:
                            // This constraint doesn't have a defined propagation.
                            break;
                        case RequiredConstraint:
                            PropagateRequiredConstraint(variable, value, constraint, ref propagation);
                            break;
                        case ExcludeConstraint:
                            PropagateExcludeConstraint(variable, value, constraint, ref propagation);
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

        private void PropagateMandatoryConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // In the case of a mandatory, if a variable is enabled, we also want to enable the other
            // variable. This means that if the variable is enabled, we need to remove 0 from the domain of the other variable.
            // We can simply swap the on/off by taking the XOR of the value with 1.
            value ^= 1;

            // Then we find the other variable.
            var parent = constraint.Variables[PARENT];
            var child = constraint.Variables[CHILD];
            var other = variable.Id == parent.Id ? child : parent;
            
            // And finally remove the value from the domain of the other variable.
            if(!other.IsSet && other.Domain.Values.Contains(value))
            {
                propagation.Add(other.Id, value);
                other.Domain.Values.Remove(value);
                propagation.IsValid = other.Domain.Values.Count > 0;
            }
        }

        private void PropagateOptionalConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // The only case that can be handled is when the child is turned on, the parent must also be turned on.
            var parent = constraint.Variables[PARENT];
            var child = constraint.Variables[CHILD];
            if(value == ON && variable.Id == child.Id)
            {
                if(!parent.IsSet && parent.Domain.Values.Contains(OFF))
                {
                    propagation.Add(parent.Id, OFF);
                    parent.Domain.Values.Remove(OFF);
                    propagation.IsValid = parent.Domain.Values.Count > 0;
                }
            }
        }

        private void PropagateAlternativeConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // In the case that the parent is enabled, we don't know which child to enable, so let the backtracker handle that.
            // In the case a child is enabled, we can disable all the other children, because there can only be one enabled.
            var parent = constraint.Variables[PARENT];
            if(variable.Id != parent.Id && value == ON)
            {
                for(int i = 1; i < constraint.Variables.Length; i++)
                {
                    var other = constraint.Variables[i];
                    if(!other.IsSet && other.Id != variable.Id && other.Domain.Values.Contains(1))
                    {
                        propagation.Add(other.Id, ON);
                        other.Domain.Values.Remove(ON);
                        if(other.Domain.Values.Count == 0)
                        {
                            propagation.IsValid = false;
                            return;
                        }
                    }
                }
                // In the case a child is enabled, we must also enable the parent.
                if(!parent.IsSet && value == ON && parent.Domain.Values.Contains(OFF))
                {
                    propagation.Add(parent.Id, OFF);
                    parent.Domain.Values.Remove(OFF);
                    propagation.IsValid = parent.Domain.Values.Count > 0;
                }
            }
        }

        private void PropagateRequiredConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // If the parent is enabled then the child must also be enabled.
            var parent = constraint.Variables[PARENT];
            var child = constraint.Variables[CHILD];
            if(variable.Id == parent.Id && value == ON)
            {
                if(!child.IsSet && child.Domain.Values.Contains(OFF))
                {
                    propagation.Add(child.Id, OFF);
                    child.Domain.Values.Remove(OFF);
                    propagation.IsValid = child.Domain.Values.Count > 0;
                }
            }
        }

        private void PropagateExcludeConstraint(Variable variable, int value, Constraint constraint, ref Propagation propagation)
        {
            // In this case, if a variable is enabled then we must disable the other variable.
            var parent = constraint.Variables[PARENT];
            var child = constraint.Variables[CHILD];
            var other = variable.Id == parent.Id ? child : parent;

            if(!other.IsSet && value == ON && other.Domain.Values.Contains(ON))
            {
                propagation.Add(other.Id, ON);
                other.Domain.Values.Remove(ON);
                propagation.IsValid = other.Domain.Values.Count > 0;
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