using System;
using System.Collections.Generic;
using System.Linq;

namespace CspSolver.Solver
{
    public abstract class Constraint
    {
        public Guid Id = Guid.NewGuid();
        public Variable[] Variables;
        protected static int ON = 1;
        protected static int OFF = 0;
        protected static int PARENT = 0;
        protected static int CHILD = 1;

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

    public class MandatoryConstraint : Constraint
    {
        public MandatoryConstraint(Variable parent, Variable child)
        {
            if(!(parent.Domain is BooleanDomain))
            {
                throw new Exception("Parent domain must be of type BooleanDomain.");
            }
            if(!(child.Domain is BooleanDomain))
            {
                throw new Exception("Child domain must be of type BooleanDomain.");
            }

            Variables = new Variable[] { parent, child };
        }

        public override bool IsSatisfied(CspModel model)
        {
            // Either 00 or 11, so not 01 or 10
            return Variables[PARENT].Value == ON && Variables[CHILD].Value == ON
                || Variables[PARENT].Value == OFF && Variables[CHILD].Value == OFF;
        }
    }

    public class OptionalConstraint : Constraint
    {
        public OptionalConstraint(Variable parent, Variable child)
        {
            if(!(parent.Domain is BooleanDomain))
            {
                throw new Exception("Parent domain must be of type BooleanDomain.");
            }
            if(!(child.Domain is BooleanDomain))
            {
                throw new Exception("Child domain must be of type BooleanDomain.");
            }

            Variables = new Variable[] { parent, child };
        }

        public override bool IsSatisfied(CspModel model)
        {
            // not allowed: parent disabled, child enabled, so 01
            return (Variables[PARENT].Value << 1 | Variables[CHILD].Value) != 0x01;
            // return Variables[0].Value == ON && Variables[1].Value == OFF
            //     || Variables[0].Value == ON && Variables[1].Value == ON
            //     || Variables[0].Value == OFF && Variables[1].Value == OFF;
        }
    }

    public class AlternativeConstraint : Constraint
    {
        public AlternativeConstraint(Variable parent, params Variable[] children)
        {
            var variables = new List<Variable> { parent };
            Variables = variables.Concat(children).ToArray();
        }

        public override bool IsSatisfied(CspModel model)
        {
            // If parent is on, then one and only one must be enabled.
            if(Variables[PARENT].Value == ON)
            {
                int count = 0;
                for(int i = 1; i < Variables.Length; i++)
                {
                    if(Variables[i].Value == ON)
                    {
                        count++;
                        if(count > 1)
                        {
                            return false;
                        }
                    }
                }
                return count == 1;
            }
            else 
            // If parent is off, then everything must be off.
            {
                for(int i = 1; i < Variables.Length; i++)
                {
                    if(Variables[i].Value == ON)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    public class OrConstraint : Constraint
    {
        public OrConstraint(Variable parent, params Variable[] children)
        {
            var variables = new List<Variable> { parent };
            Variables = variables.Concat(children).ToArray();
        }

        public override bool IsSatisfied(CspModel model)
        {
            // If parent is on, then at least one must be enabled.
            if(Variables[PARENT].Value == ON)
            {
                int count = 0;
                for(int i = 1; i < Variables.Length; i++)
                {
                    if(Variables[i].Value == ON)
                    {
                        count++;
                    }
                }
                return count > 0;
            }
            else 
            // If parent is off, then everything must be off.
            {
                for(int i = 1; i < Variables.Length; i++)
                {
                    if(Variables[i].Value == ON)
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    public class RequiredConstraint : Constraint
    {
        public RequiredConstraint(Variable parent, Variable child)
        {
            if(!(parent.Domain is BooleanDomain))
            {
                throw new Exception("Parent domain must be of type BooleanDomain.");
            }
            if(!(child.Domain is BooleanDomain))
            {
                throw new Exception("Child domain must be of type BooleanDomain.");
            }

            Variables = new Variable[] { parent, child };
        }

        public override bool IsSatisfied(CspModel model)
        {
            // if parent is enabled, child must be enabled   11
            // if parent is disable, child can be off/on     01 | 00 
            // value that is not allowed is                  10
            return (Variables[PARENT].Value << 1 | Variables[CHILD].Value) != 0x10;
        }
    }

    public class ExcludeConstraint : Constraint
    {
        public ExcludeConstraint(Variable parent, Variable child)
        {
            if(!(parent.Domain is BooleanDomain))
            {
                throw new Exception("Parent domain must be of type BooleanDomain.");
            }
            if(!(child.Domain is BooleanDomain))
            {
                throw new Exception("Child domain must be of type BooleanDomain.");
            }

            Variables = new Variable[] { parent, child };
        }

        public override bool IsSatisfied(CspModel model)
        {
            // parent on then child off       10
            // parent off the child on/off    01 | 00 
            // value that is not allowed is   11
            return (Variables[PARENT].Value << 1 | Variables[CHILD].Value) != 0x11;
        }
    }
}