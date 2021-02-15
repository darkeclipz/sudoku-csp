using System;

namespace CspSolver.Solver
{
    public class Variable
    {
        public int Id;
        public string Name;
        public Domain Domain;
        public int Value;
        public bool IsSet;

        public void AddDomain(Domain domain)
        {
            Domain = Domain.Copy(domain);
        }

        public void Assign(int value)
        {
            Value = value;
            IsSet = true;
            Domain.Values.Remove(value);
        }

        public void Unassign(int value)
        {
            Value = 0;
            IsSet = false;
            Domain.Values.Add(value);
        }
    }
}