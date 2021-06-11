        private SearchState Search()

        {

            var stack = new Stack<(int, int)>();

            var dataStack = new Stack<(int, Propagation)>();

           

            var id = NextVariable();

            var values = OrderedValues(Model.Variables[id]);

 

            foreach(var value in values/*.Reverse()*/)

            {

                stack.Push((id, value));  

            }

 

            Console.WriteLine("Searching...");

            while(stack.Count > 0)

           {

                (var var, var value) = stack.Pop();

 

                if(value >= 0)

                {

                    var variable = Model.Variables[var];

                    variable.Assign(value);

                    Statistics.TotalAssigments++;

                    var propagation = ConstraintPropagator.PropagateAssignment(variable, value);

                    stack.Push((var, UNASSIGN));

                    dataStack.Push((value, propagation));

 

                    if (propagation.IsValid)

                    {

                        var nextVariable = NextVariable();

                        if (nextVariable == ALL_ASSIGNED)

                        {

                            return SearchState.Satisfied;

                        }

 

                        var nextValues = Model.Variables[nextVariable].Domain.Values; // OrderedValues(Model.Variables[nextVariable]);

                        foreach (var nextValue in nextValues/*.Reverse()*/)

                        {

                            stack.Push((nextVariable, nextValue));

                        }

                    }

                }

                else

                {

                    (var x, var prop) = dataStack.Pop();

                    var variable = Model.Variables[var];

                    variable.Unassign(x);

                    ConstraintPropagator.UndoPropagation(prop);

                }

            }

 

            return SearchState.Infeasible;

        }
