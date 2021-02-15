using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    public interface ISolver
    {
        Model Model { get; set; }
        bool Solve();
    }

    public class BacktrackSolver
    {
        public Model Model { get; set; }
        private Guid[] variableKeys;

        public BacktrackSolver(Model model)
        {
            this.Model = model;
        }

        // Check every set variable, and remove this value from all the other variable domains
        // which share the same constraint.
        public void Propagate()
        {
            foreach(var variable in Model.Variables)
            {
                if(variable.Value.IsSet)
                {
                    var constraintsWithVariable = Model.Constraints.Where(c => c.Value.VariableKeys.Contains(variable.Key));
                    foreach(var constraint in constraintsWithVariable)
                    {
                        foreach(var otherVariable in constraint.Value.VariableKeys)
                        {
                            if(variable.Key == otherVariable)
                            {
                                continue;
                            }

                            if(variable.Value.Domain.Id == Model.Variables[otherVariable].Domain.Id)
                            {
                                if(Model.Variables[otherVariable].Domain.Values.Contains(variable.Value.Value))
                                {
                                    Model.Variables[otherVariable].Domain.Values.Remove(variable.Value.Value);
                                }
                            }
                        }
                    }
                }
            }
        }

        // Prune all the domains and check if there are variables with a single
        // value remaining in the domain. Assign this value to the variable, and
        // prune all the domains again. Repeat this until there are no more variables
        // with a single domain.
        public void MakeArcConsistent()
        {
            Propagate();
            var singleDomainVariables = VariablesWithSingleDomain();
            while(singleDomainVariables.Count > 0)
            {
                foreach(var variable in singleDomainVariables)
                {
                    var value = Model.Variables[variable].Domain.Values.First();
                    Model.Variables[variable].Assign(value);
                }
                Propagate();
                singleDomainVariables = VariablesWithSingleDomain();
            }
        }

        // Get all the variables which only have a single value in the domain.
        public List<Guid> VariablesWithSingleDomain()
        {
            var list = new List<Guid>();
            foreach(var variable in Model.Variables)
            {
                if(variable.Value.Domain.Values.Count == 1)
                {
                    list.Add(variable.Key);
                }
            }
            return list;
        }

        public bool Solve()
        {
            variableKeys = GetVariablesInOrder(VariableOrdering.SmallestDomain).ToArray();
            int n = variableKeys.Length;
            BacktrackState state = Backtrack(0, n);
            return state == BacktrackState.Solved;
        }

        // Order the variables based on a heuristic which is then used
        // as the order in which the variables are solved.
        private List<Guid> GetVariablesInOrder(VariableOrdering order)
        {
            var count = new Dictionary<Guid, int>();
            foreach(var variable in Model.Variables)
            {
                count.Add(variable.Key, 0);
            }

            if(order == VariableOrdering.MostConstrained || order == VariableOrdering.LeastConstrained)
            {
                foreach(var constraint in Model.Constraints)
                {
                    foreach(var variable in constraint.Value.VariableKeys)
                    {
                        count[variable]++;
                    }
                }
            }
            else if(order == VariableOrdering.BiggestDomain || order == VariableOrdering.SmallestDomain)
            {
                foreach(var variable in Model.Variables)
                {
                    count[variable.Key] = variable.Value.Domain.Values.Count;
                }
            }

            if(order == VariableOrdering.MostConstrained || order == VariableOrdering.BiggestDomain)
            {
                return count.OrderByDescending(c => c.Value).Select(c => c.Key).ToList();
            }
            else if(order == VariableOrdering.LeastConstrained || order == VariableOrdering.SmallestDomain)
            {
                return count.OrderBy(c => c.Value).Select(c => c.Key).ToList();
            }
            else
            {
                throw new Exception("Invalid ordering.");
            }
        }

        // Try every domain value for all the variables until the entire
        // solution is found.
        private BacktrackState Backtrack(int current, int n)
        {
            var state = BacktrackState.Unsolved;

            // Check if the base case, which is when all the variables
            // are set, is a valid solution.
            if(current == n)
            {
                if(Model.IsSatisfied(allowPartialAssignment: false))
                {
                    return BacktrackState.Solved;
                }
                else
                {
                    return BacktrackState.Unsolved;
                }
            }

            var variable = Model.Variables[variableKeys[current]];

            // Check if this variable is already set.
            if(variable.IsSet)
            {
                return Backtrack(current + 1, n);
            }

            // Assign each value in the domain to the variable, and check
            // if this gives a valid solution.
            var domain = Domain.Copy(variable.Domain);
            foreach(var value in domain.Values)
            {
                variable.Assign(value);

                Console.WriteLine("".PadLeft(current * 2, ' ') + $"Set variable '{variable.Name}' to value '{value}'");

                if(Model.IsSatisfied(allowPartialAssignment: true))
                {
                    state = Backtrack(current + 1, n);
                    if(state == BacktrackState.Solved)
                    {
                        break;
                    }
                }
                variable.Unassign(value);
                Console.WriteLine("".PadLeft(current * 2, ' ') + $"Unset variable '{variable.Name}'.");
            }

            return state;
        }
    }

    public enum BacktrackState
    {
        Unsolved,
        Solved
    }

    // Order in which the variables are solved.
    public enum VariableOrdering
    {
        MostConstrained,
        LeastConstrained,
        SmallestDomain,
        BiggestDomain
    }
}