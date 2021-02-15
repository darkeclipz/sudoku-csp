using CspSolver.Solver;

namespace CspSolver.Models
{
    public class PhoneFeatureModel
    {
        public static ModelBuilder GetModel()
        {
            var builder = new ModelBuilder();
            var domain = builder.CreateBooleanDomain();

            var mobilePhone = builder.CreateVariable("Mobile Phone", domain);
            var calls = builder.CreateVariable("Calls", domain);
            var gps = builder.CreateVariable("GPS", domain);
            var screen = builder.CreateVariable("Screen", domain);
            var basicScreen = builder.CreateVariable("Basic", domain);
            var colourScreen = builder.CreateVariable("Colour", domain);
            var highResolutionScreen = builder.CreateVariable("High resolution", domain);
            var media = builder.CreateVariable("Media", domain);
            var camera = builder.CreateVariable("Camera", domain);
            var mp3 = builder.CreateVariable("MP3", domain);

            builder.CreateMandatoryConstraint(mobilePhone, calls);
            builder.CreateOptionalConstraint(mobilePhone, gps);
            builder.CreateMandatoryConstraint(mobilePhone, screen);
            builder.CreateOptionalConstraint(mobilePhone, media);
            builder.CreateAlternativeConstraint(screen, basicScreen, colourScreen, highResolutionScreen);
            builder.CreateOrConstraint(media, camera, mp3);
            builder.CreateExcludeConstraint(gps, basicScreen);
            builder.CreateRequiredConstraint(camera, highResolutionScreen);

            mobilePhone.Assign(1);
            // camera.Assign(1);
            gps.Assign(1);
            basicScreen.Assign(1);

            return builder;
        }
    }
}