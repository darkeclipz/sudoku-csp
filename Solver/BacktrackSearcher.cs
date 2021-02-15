using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CspSolver.Solver
{
    // Solves a CSP model with a recursive backtracking algorithm using forward propagation
    // and different heuristics to reduce the search space.
    public class BacktrackSearcher
    {
        public CspModel Model;
        public BacktrackSearchStatistics Statistics = new BacktrackSearchStatistics();
        public VariableOrderHeuristic VariableOrder = VariableOrderHeuristic.SmallestValueOrder;
        public ValueOrderHeuristic ValueOrder = ValueOrderHeuristic.LexicographicalOrder;

        private ConstraintPropagator ConstraintPropagator;
        private int ALL_ASSIGNED = -1;

        public BacktrackSearcher(CspModel model)
        {
            Model = model;
            ConstraintPropagator = new ConstraintPropagator(model);
        }

        // Propagate all the initial assigned variables to reduce
        // the domains of all the variables that share the same constraints.
        private bool PropagateAssignedVariables()
        {
            for(int i = 0; i < Model.Variables.Length; i++)
            {
                var variable = Model.Variables[i];
                if(variable.IsSet)
                {
                    var propagation = ConstraintPropagator.PropagateAssignment(variable, variable.Value);
                    if(!propagation.IsValid)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        // Solve the CSP model.
        public SearchState Solve()
        {
            Statistics.Stopwatch.Start();

            // First we want to propagate all the initially assigned variables.
            if(!PropagateAssignedVariables())
            {
                return SearchState.Infeasible;
            }
            // Solve the CSP model with a recursive backtracking algorithm.
            var state = Search();

            Statistics.Stopwatch.Stop();

            // If the model is still undetermined at the end of the search, then
            // the model is infeasible.
            if(state == SearchState.Undetermined)
            {
                return SearchState.Infeasible;
            }

            return state;
        }

        private SearchState Search()
        {
            // Get the next variable that should be assigned.
            var index = NextVariable();

            // If all the variables are assigned, then there is no next variable
            // and we need to check if the model is consistent with all the constraints.
            if(index == ALL_ASSIGNED)
            {
                if(Model.IsConsistent())
                {
                    return SearchState.Satisfied;
                }
                else
                {
                    return SearchState.Undetermined;
                }
            }

            // Get the variable (for easier access), and the values in the order
            // in which they should be assigned.
            var variable = Model.Variables[index];
            var values = OrderedValues(variable);
            for(int i = 0; i < values.Length; i++)
            {
                var value = values[i];
                variable.Assign(value);
                Statistics.TotalAssigments++;
               
                // Propagate the assigned value, which means that this value is removed
                // from all variables that share the same constraints. If a domain of a variable
                // is reduced to zero, then we have assigned an invalid value, and we backtrack early.
                var propagation = ConstraintPropagator.PropagateAssignment(variable, value);
                if(propagation.IsValid)
                {
                    // Proceed to the next variable.
                    var state = Search();

                    // If the model is satisfied, we can abort the search, and return
                    // the found solution.
                    if(state == SearchState.Satisfied)
                    {
                        return state;
                    }
                }

                // Undo all the changes to return to the state before the assigment of
                // the variable.
                ConstraintPropagator.UndoPropagation(propagation);
                variable.Unassign(value);
            }

            return SearchState.Undetermined;
        }

        // Returns the next variable to be solved based on the selected heuristic.
        private int NextVariable()
        {
            switch(VariableOrder)
            {
                case VariableOrderHeuristic.SmallestValueOrder:
                    return VariableWithMinimumRemainingValues();
                default:
                    throw new Exception("Variable order not supported.");
            }
        }

        // Returns the variable with the minimum remaining values in the domain.
        // This ensures that the search space is reduced quickly by selecting
        // the variable that has the least values remaining, so the algorithm
        // will fail quickly.
        public int VariableWithMinimumRemainingValues()
        {
            int min = int.MaxValue;
            int id = ALL_ASSIGNED;
            for(int i = 0; i < Model.Variables.Length; i++)
            {
                var variable = Model.Variables[i];
                if(!variable.IsSet && variable.Domain.Values.Count < min)
                {
                    min = variable.Domain.Values.Count;
                    id = i;
                }
            }
            return id;
        }

        // Returns the order of the values in which they should be assigned.
        private int[] OrderedValues(Variable variable)
        {
            switch(ValueOrder) 
            {
                case ValueOrderHeuristic.LexicographicalOrder:
                    return variable.Domain.Values.ToArray();
                case ValueOrderHeuristic.LeastConstrainedOrder:
                    return LeastConstrainingValueOrder(variable);
                default:
                    throw new Exception("Value order not supported.");
            }
        }

        // Returns the values in the least constrained order. This gives maximum
        // flexibility (more choices) when solving in later steps.
        private int[] LeastConstrainingValueOrder(Variable variable)
        {
            var order = new Dictionary<int, int>();
            foreach(var value in variable.Domain.Values)
            {
                var count = CountValueSharedInConstraints(variable, value);
                order.Add(value, count);
            }
            return order.OrderBy(o => o.Value).Select(o => o.Key).ToArray();
        }

        // Returns how many times a value is shared with constraints. These are the
        // number of values that the will removed from the domains when the assignment
        // is propagated.
        private int CountValueSharedInConstraints(Variable variable, int value)
        {
            int count = 0;
            for(int i = 0; i < Model.Constraints.Length; i++)
            {
                var constraint = Model.Constraints[i];
                if(constraint.ContainsVariable(variable.Id))
                {
                    for(int j = 0; j < constraint.Variables.Length; j++)
                    {
                        if(!constraint.Variables[j].IsSet && constraint.Variables[j].Domain.Values.Contains(value))
                        {
                            count++;
                        }
                    }
                }
            }
            return count;
        }
    }

    // Determines the state of the model.
    public enum SearchState
    {
        Undetermined,
        Infeasible,
        Satisfied
    }

    // Determines in which order the variables are sovled.
    public enum VariableOrderHeuristic
    {
        SmallestValueOrder
    }

    // Determines in which order the values are assigned.
    public enum ValueOrderHeuristic 
    {
        LexicographicalOrder,
        LeastConstrainedOrder
    }
}