using Foundation;
using IX15Configurator.iOS.Services;
using IX15Configurator.Services;

[assembly: Xamarin.Forms.Dependency(typeof(AppVersionDependencyService))]
namespace IX15Configurator.iOS.Services
{
    class AppVersionDependencyService : IAppVersionDependencyService
    {
        public string GetName()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleDisplayName").ToString();
        }

        public string GetVersion()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleShortVersionString").ToString();
        }

        public string GetBuild()
        {
            return NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
        }
    }
}