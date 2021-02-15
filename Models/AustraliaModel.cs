using CspSolver.Solver;

namespace CspSolver.Models
{
    public class AustraliaModel
    {
        static int Red = 0;
        static int Green = 1;
        static int Blue = 2;

        public static Model GetModel()
        {
            var model = new Model();
            var domain = model.CreateDomain("colors", Red, Green, Blue);
            var wa = model.CreateVariable("Western Australia", domain);
            var nt = model.CreateVariable("Northern Territory", domain);
            var sa = model.CreateVariable("South Astralia", domain);
            var qe = model.CreateVariable("Queensland", domain);
            var nsw = model.CreateVariable("New South Wales", domain);
            var vi = model.CreateVariable("Victoria", domain);
            model.CreateAllDifferentConstraint(wa, nt);
            model.CreateAllDifferentConstraint(wa, sa);
            model.CreateAllDifferentConstraint(nt, sa);
            model.CreateAllDifferentConstraint(nt, qe);
            model.CreateAllDifferentConstraint(sa, qe);
            model.CreateAllDifferentConstraint(sa, nsw);
            model.CreateAllDifferentConstraint(sa, vi);
            model.CreateAllDifferentConstraint(nsw, vi);
            return model;
        }
    }
}